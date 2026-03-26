using FalveyInsuranceGroup.Backend.Dtos;
using FalveyInsuranceGroup.Backend.Models;
using FalveyInsuranceGroup.Backend.Services;
using FalveyInsuranceGroup.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NTS = NetTopologySuite.Geometries;

namespace FalveyInsuranceGroup.Backend.Controllers
{
    /// <summary>
    /// Handles operations releated to location data and geosearch queries
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MapsController : ControllerBase
    {

        private readonly FalveyInsuranceGroupContext _context;
        private readonly MapService _mapService;
        public MapsController(FalveyInsuranceGroupContext context, MapService mapService)
        {
            _context = context;
            _mapService = mapService;
        }


        /// <summary>
        /// Sends front-end's query to MapService to get any locations nearby
        /// </summary>
        /// <param name="location">Query that holds basic geographical information</param>
        /// <returns>List of location(s)</returns>
        /// <response code="400">Request is invalid</response>
        [HttpPost("search")]
        public async Task<ActionResult> getNearbyLocations([FromBody] GeosearchDto location)
        {
            try
            {
                var locations = await _mapService.geoSearchAsync(location);
                return Ok(locations);
            }
            catch (ArgumentException e)
            {
                return BadRequest(new { message = e.Message });
            }
        }


        /// <summary>
        /// Get all locations
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<Location>> getLocations()
        {
            var location = await _context.Locations
            .AsNoTracking()
            .Include(l => l.customer)
            .Select(l => new Location
            {
                location_id = l.location_id,
                address = l.address,
                latitude = l.latitude,
                longitude = l.longitude,
                customer_id = l.customer_id,
                location = l.location,
                cope_eval = l.cope_eval,
                site_eval = l.site_eval,
                cope_date = l.cope_date,
                site_date = l.site_date
            }).
            ToListAsync();

            return location;
        }


        /// <summary>
        /// Gets a specific location by its ID
        /// </summary>
        /// <param name="id">Location ID</param>
        /// <returns>Location with associated ID</returns>
        /// <response code="200">Successful location query</response>
        /// <response code="404">Location not found</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Location>> getLocationById(int id)
        {
            var location = await _context.Locations
            .AsNoTracking()
            .Include(l => l.customer)
            .FirstOrDefaultAsync(l => l.location_id == id);

            if (location == null)
            {
                return NotFound($"Location with ID {id} not found");
            }

            return Ok(location);
        }


        /// <summary>
        /// Gets a specific location by customer ID
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <returns>Location with associated customer ID</returns>
        /// <response code="200">Successful location query</response>
        /// <response code="404">Location not found</response>
        [HttpGet("customer/{id}")]
        public async Task<ActionResult<Location>> getLocationByCustomerId(int id)
        {
            var location = await _context.Locations
            .AsNoTracking()
            .Include(l => l.customer)
            .Where(l => l.customer_id == id)
            .ToListAsync();

            if (location == null)
            {
                return NotFound($"Location with Customer ID {id} not found");
            }

            return Ok(location);
        }

        /// <summary>
        /// Adds new location to database
        /// </summary>
        /// <param name="loc">Location Dto that holds user's request</param>
        /// <returns>The new location</returns>
        /// <response code="201">Location added successfully</response>
        /// <response code="400">Invalid parameters</response>
        [HttpPost("add")]
        public async Task<ActionResult<Location>> addLocationAsync([FromBody] LocationDto loc)
        {
             // Ensure address does not already exist in database
            if(await doesAddressExist(loc.address))
            {
                return BadRequest(new {error = "Address already in use."});
            }
            
            // Validate input for COPE assessment.
            if ((loc.cope_eval != false && loc.cope_date == null) || (loc.cope_eval == false && loc.cope_date != null))
            {
                return BadRequest(new { error = "COPE evaluation input not valid." });
            }

            // Validate input for on-site assessment.
            if ((loc.site_eval != false && loc.site_date == null) || (loc.site_eval == false && loc.site_date != null))
            {
                return BadRequest(new { error = "On-site evaluation input not valid." });
            }

            // Validate date for assessments.
            var cutoff_date = DateOnly.FromDateTime(DateTime.Today).AddMonths(-18);
            if (loc.cope_date < cutoff_date || loc.site_date < cutoff_date)
            {
                return BadRequest(new { error = "Date is out of range." });
            }

            // Validate customer exists.
            if (!await doesCustomerExist(loc.customer_id))
            {
                return BadRequest("The given customer ID does not exist");
            }

            // Use NTS Point for spatial objects if we have time in future
            var new_loc = new Location
            {
                address = loc.address,
                latitude = loc.latitude,
                longitude = loc.longitude,
                customer_id = loc.customer_id,
                cope_eval = loc.cope_eval,
                site_eval = loc.site_eval,
                cope_date = loc.cope_date,
                site_date = loc.site_date,
                location =  new NTS.Point(loc.latitude, loc.longitude) { SRID = 4326 } 
            };

            _context.Locations.Add(new_loc);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(getLocationById), new { id = new_loc.location_id}, new_loc);
        }

        /// <summary>
        /// Updates an existing location by ID
        /// </summary>
        /// <param name="id">Location Dto that holds user's request</param>
        /// <param name="updated_location">Location object that holds the new details</param>
        /// <returns>The updated location</returns>
        /// <response code="201">Location added successfully</response>
        /// <response code="400">Invalid parameters</response>
        [HttpPut("{id}")]
        public async Task<ActionResult> updateLocation(int id, [FromBody] LocationDto updated_location)
        {
            var location = await _context.Locations.FindAsync(id);
            if(updated_location.address != location.address)
            {
                if(await doesAddressExist(updated_location.address))
                {
                    return BadRequest(new {error = "Address already in use."});
                }
            }
            
            // Validate input for COPE assessment.
            if ((updated_location.cope_eval != false && updated_location.cope_date == null) || (updated_location.cope_eval == false && updated_location.cope_date != null))
            {
                return BadRequest(new { error = "COPE evaluation input not valid." });
            }

            // Validate input for on-site assessment.
            if ((updated_location.site_eval != false && updated_location.site_date == null) || (updated_location.site_eval == false && updated_location.site_date != null))
            {
                return BadRequest(new { error = "On-site evaluation input not valid." });
            }

            // Validate date for assessments.
            var cutoff_date = DateOnly.FromDateTime(DateTime.Today).AddMonths(-18);
            if (updated_location.cope_date < cutoff_date || updated_location.site_date < cutoff_date)
            {
                return BadRequest(new { error = "Date is out of range." });
            }

            // Validate customer exists.
            if (!await doesCustomerExist(updated_location.customer_id))
            {
                return BadRequest("The given customer ID does not exist");
            }

            // Update fields
            // Use NTS Point for spatial objects if we have time in future
            location.address = updated_location.address;
            location.latitude = updated_location.latitude;
            location.longitude = updated_location.longitude;
            location.customer_id = updated_location.customer_id;
            location.cope_eval = updated_location.cope_eval;
            location.site_eval = updated_location.site_eval;
            location.cope_date = updated_location.cope_date;
            location.site_date = updated_location.site_date;
            location.location = new NTS.Point(updated_location.latitude, updated_location.longitude) { SRID = 4326 };

            await _context.SaveChangesAsync();

            return Ok(location);
        }

        /// DELETE: api/maps/id

        /// <summary>
        /// Deletes an existing location
        /// </summary>
        /// <param name="id">The location to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">Deletion was a success</response>
        /// <response code="404">Location was not found</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteLocation(int id)
        {
            var location = await _context.Locations.FindAsync(id);

            // Check if recommendation exists.
            if (location == null)
            {
                return NotFound($"Location with ID {id} not found");
            }

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();
            return Ok(location.address);
            //return NoContent();
        }

        /// <summary>
        /// Checks to see if a customer exists in the database
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <returns>The customer that is assigned to the ID</returns>
        private async Task<bool> doesCustomerExist(int id)
        {
            return await _context.Customers.AnyAsync(c => c.customer_id == id);
        }

        /// <summary>
        /// Checks to see if an address exists in the database
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns>The truth value of whether address exists</returns>
        private async Task<bool> doesAddressExist(string address)
        {
            return await _context.Locations.AnyAsync(l => l.address == address);
        }
        
    }
}
