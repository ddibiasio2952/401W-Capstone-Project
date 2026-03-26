/* DAN EDIT 11/27/2025 PM */

let map;
let marker;
let location_markers = [];

const search_bar = document.querySelector(".search-box input");
const loc_list = document.querySelector(".location-list");

// Initializes map on page start-up
function renderMap() {
    try {
        const map_prop = {
            mapId: "b4899a90d947649f1820ad83",
            center: { lat: 40.0, lng: -103.0 },
            zoom: 4,
        };

        map = new google.maps.Map(document.querySelector(".map-placeholder"), map_prop);

        // Check URL for search param to render
        const params = new URLSearchParams(window.location.search);
        let searchString = params.get('searchstring');


        // Show region results if there is a search string
        if (searchString) {
            searchBarLocation(searchString);
        }

    } catch (error) {
        console.error("ERROR", error);
    }
}

// Geocodes to Google Maps Geocoding API
async function geocodeLocation(address) {
    const geocoder = new google.maps.Geocoder();
    const loc_request = { address };

    return new Promise((Resolve, Reject) => {
        geocoder.geocode(loc_request, (results, status) => {
            if (status === 'OK') {
                Resolve(results[0]);
            } else {
                Reject(new Error("Geocode was unsuccessful: ", status));

                console.warn("Invalid Location");

                // Failure clears location list and markers
                loc_list.innerHTML = "No locations found";
                if (hasArrayElements(location_markers)) {
                    clearLocationMarkers();
                }
            }
        });
    });
}

// Creates location cards to add to location list
async function addLocationsToList(locations) {
    // Clear active marker
    active_marker = null;
    
    // Clear location list

    const table = document.querySelector('#location-list');
    table.innerHTML = ''; // Clear existing rows
    for (const location of locations) {
        // Get COPE & On-Site assessments from Maps API
        const assessments = await getLocation(location.location_id);

        const row = document.createElement('div');
        row.className = 'location-card'; // Assign class="location-card" to created elements for CSS formatting
        row.innerHTML = `
            <div class="location-info">
            <p>${location.address}</p>
            <a href="../Location/customer-location.html?customer_id=${location.customer_id}&location_id=${location.location_id}" class="view-link"><button class="openModalBtn">View More Details</button></a>
            <button class="openModalBtn" onclick="openLocationEditForm(${location.location_id})">Edit</button>
            </div>
             <div class="status-grid">
             ${assessments[0]}
             ${assessments[1]}
             </div>
            `;
        // Delete Location Button (Removed for Security)
        // <button class="openModalBtn" onclick='deleteLocation(${location.location_id}, ${JSON.stringify(location.address)})'>Delete</button>
        row.addEventListener("click", () => {
            activateCard(row);
            activateMarker(location.location_id);
        });
        // Add to table
        table.appendChild(row);
    };
}

// When a user selects a card, it focuses on it by adding a border
let previous_card = null;
function activateCard(loc_card) {

    // If card is re-selected, remove border and return
    if (previous_card === loc_card) {
        previous_card.classList.remove("selected");
        previous_card = null;
        return;
    }

    // If previous card is not the same selected card, remove its property anyways
    if (previous_card) {
        previous_card.classList.remove("selected");
    }

    loc_card.classList.add("selected");
    previous_card = loc_card;
}

// When a card is selected, it pans to associated marker and adds a bounce animation 
let active_marker = null;
let previous_marker = null;
function activateMarker(location_id) {
    const marker = map.get(location_id);

    // Removes bounce animation of previous marker when user interacts w/ other cards
    if (previous_marker) {
        previous_marker.content.classList.remove("bounce-animation");
    }

    if (!marker) {
        console.warn(`Marker ${location_id} does not exist`);
        return;
    }

    // Compares card ID to marker ID to see if they are associated
    // If true, user de-focuses the associated marker and location on map
    if (location_id === active_marker) {
        map.setZoom(12);
        active_marker = null;
        return;
    }

    // If card ID and marker ID are not the same,
    // User wants to focus on card's associated marker and location on map
    map.setZoom(15);
    map.panTo(marker.position);
    marker.content.classList.add("bounce-animation");

    // Saves current marker information for next function call
    previous_marker = marker;
    active_marker = location_id;
}

// Adds markers to Google Map
function addMarkersToMap(locations) {
    const bounds = new google.maps.LatLngBounds();

    locations.forEach(location => {
        if (location.latitude === undefined || location.longitude === undefined) return;

        const anchor = new google.maps.marker.PinElement({
            background: "#2c4275",
            borderColor: "#2c4275",
            glyphSrc: "../image/anchor.jpg",
            scale: 1.3,
        });

        anchor.element.classList.add("drop-animation");

        const marker = new google.maps.marker.AdvancedMarkerElement({
            map,
            title: location.address,
            position: { lat: location.latitude, lng: location.longitude },
            content: anchor.element,
        });

        // Assigns an id to marker
        map.set(location.location_id, marker);

        const customer_link = `https://localhost:7288/Location/customer-location.html?location_id=${location.location_id}`;

        const info_window = new google.maps.InfoWindow({
            content: `
            <div style="font-size:14px; max-width:200px; padding: 5px;">
                <b>${location.customer.name || "N/A"}</b>
                <br>
                ${location.address || "No address available"}<br>
                <a href="${customer_link}" style="font-weight:500; font-size:13px;">View Details</a>
            </div>`,
        });

        marker.addEventListener("click", () => info_window.open(map, marker));

        bounds.extend(marker.position);

        location_markers.push(marker);
    });
    map.fitBounds(bounds);



}


// Clears markers from Google Map
function clearLocationMarkers() {
    for (const marker of location_markers) {
        marker.map = null;
    }
    location_markers = [];
}

// Checks to see if a query is a region
function isRegion(loc_types) {
    return (loc_types.includes("country")
        || loc_types.includes("administrative_area_level_1")
        || loc_types.includes("administrative_area_level_2")
    );
}

// Dynamically sets zoom level for map search based on location
function setZoomLevel(loc_types) {
    if (loc_types.includes("country")) {
        map.setZoom(4);
    } else if (loc_types.includes("administrative_area_level_1")) {
        map.setZoom(7);
    } else if (loc_types.includes("administrative_area_level_2")) {
        map.setZoom(9);
    } else {
        map.setZoom(10);
    }
}

// Checks to see if there are elements in an array
function hasArrayElements(array) {
    return array && array.length > 0;
}

// Handles user input for geo search
search_bar.addEventListener("keydown", async (event) => {
    if (event.key === "Enter") {
        const address_input = search_bar.value;

        if (!address_input || address_input.trim() === "") {
            loc_list.innerHTML = "No locations found";
            console.warn("Invalid location");

            if (hasArrayElements(location_markers))
                clearLocationMarkers();

            return;
        }

        try {
            const georesult = await geocodeLocation(address_input);
            const location = georesult.geometry.location;
            const loc_types = georesult.types;
            const bounds = georesult.geometry.viewport;

            const search_type = isRegion(loc_types) ? "region" : "proximity";

            const request = {
                search_type: search_type,
                latitude: location.lat(),
                longitude: location.lng(),
            };

            // Extracts viewport w/ bounding box if available
            if (search_type === "region") {
                if (bounds && bounds.getNorthEast && bounds.getSouthWest) {
                    const ne = bounds.getNorthEast();
                    const sw = bounds.getSouthWest();

                    request.min_latitude = sw.lat();
                    request.max_latitude = ne.lat();
                    request.min_longitude = sw.lng();
                    request.max_longitude = ne.lng();
                }
            }

            // Updates map to reflect location query
            map.panTo(location);
            setZoomLevel(loc_types);
            //console.log("HERE***: ", request);

            // Queries to backend for a response
            const response = await fetch("https://localhost:7288/api/maps/search", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(request)
            });

            if (!response.ok) {
                throw new Error("HTTP request failed: ", response.status);
            }

            const nearbyLocations = await response.json();

            clearLocationMarkers();
            // Checks to see if response has any locations
            if (hasArrayElements(nearbyLocations)) {
                await addLocationsToList(nearbyLocations);
                addMarkersToMap(nearbyLocations);
            } else {
                loc_list.innerHTML = "No locations found";
            }

        } catch (error) {
            console.error("ERROR:", error);
        }
    }
});

/* CUSTOMER MODAL FORM */
const addCustomerButtons = document.querySelectorAll(".add-customer-btn");
const customerModal = document.getElementById("addCustomerModal");
const customerInputForm = document.getElementById("addCustomerForm");
const customerEditButton = document.querySelector('#submit_customer_btn');
const customerEditor = document.getElementById('customer-editor');
const customerPolicyIdField = document.getElementById('customer_policy_id');
const customerId = document.createElement('div');

function openCustomerForm() {
    customerModal.classList.add('active');
}
function closeCustomerForm() {
    customerModal.classList.remove('active');
    document.getElementById('addCustomerForm').reset();
    customerId.innerHTML = '';
}

/* GET CUSTOMERS */
async function getCustomers() {
    try {
        const response = await fetch(`https://localhost:7288/api/customers`);

        // Return error if response fails
        if (!response.ok) {
            const backError = await response.text();
            alert(backError);
            alert('Unable to get employee.');
            throw new Error(backError);
        }

        // Assign JSON object in a variable
        const customers = await response.json();

        // Return error if response is empty
        if (customers.length == 0) {
            console.error(`Customer does not exist in database.`);
        }
        locationCustomer.innerHTML = ''; // Clear existing dropdown menu
        // Populate location select table with data
        for (const customer of customers) {
            const row = document.createElement('option');

            row.value = customer.customer_id;
            row.textContent = customer.name;
            locationCustomer.appendChild(row);
        };

    } catch (error) {
        console.error(`Error: ${error}`);
    }
}

/* ADD NEW CUSTOMER */
async function addCustomer() {

    const add_created_at = new Date(Date.now() - 5 * 60 * 60 * 1000);

    const customer = {
        name: document.getElementById('name').value,
        email: document.getElementById('email').value,
        phone: document.getElementById('phone').value,
        addr_line1: document.getElementById('addr_line1').value,
        addr_line2: document.getElementById('addr_line2').value,
        city: document.getElementById('city').value,
        state_code: document.getElementById('state_code').value,
        zip_code: document.getElementById('zip_code').value,
        created_at: add_created_at
    };
    console.log('Submission:', JSON.stringify(customer));
    fetch('https://localhost:7288/api/customers', {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(customer)
    })
        .then(async response => {
            if (!response.ok) {
                const backError = await response.text();
                alert(backError);
                alert('Unable to add customer. ');
                throw new Error(backError);
            }
            return response.json();
        })
        .then(form_data => {
            alert('New customer ID: ' + form_data.customer_id);
            localStorage.setItem('customer_id', form_data.customer_id);
            closeCustomerForm();
            getCustomers();
        })
        .catch(error => {
            console.error('Unable to add customer.', error);
        })
}

/* LOCATION MODAL FORM */
const addLocationButtons = document.querySelectorAll(".add-location-btn");
const locationModal = document.getElementById("addLocationModal");
const locationInputForm = document.getElementById("addLocationForm");
const locationEditButton = document.querySelector('#submit_location_btn');
const locationPolicyIdField = document.getElementById('location_policy_id');
const locationId = document.createElement('div');
const locationCustomer = document.getElementById('loc_customer_id');
function openLocationForm() {
    locationModal.classList.add('active');
}

async function openLocationEditForm(location_id) {
    try {
        const response = await fetch(`https://localhost:7288/api/maps/${location_id}`);
        // Return error if response fails
        if (!response.ok) {
            const backError = await response.text();
            alert(backError);
            alert('Unable to get location.');
            throw new Error(backError);
        }

        // Assign JSON object in a variable
        const location = await response.json();

        // Return error if response is empty
        if (location.length == 0) {
            console.error('Location does not exist in database.');
        }
        // Send information to modal form
        // Run delimiter on Location Address
        const address = location.address;
        let addressSubstrings = address.split(", ").map(part => part.trim());
        console.log('Length:', addressSubstrings.length);
        if (addressSubstrings.length == 5) {
            document.getElementById('loc_addr_line1').value = addressSubstrings[0];
            document.getElementById('loc_addr_line2').value = addressSubstrings[1];
            document.getElementById('loc_city').value = addressSubstrings[2];
            document.getElementById('loc_state_code').value = addressSubstrings[3];
            document.getElementById('loc_zip_code').value = addressSubstrings[4];
        }
        else {
            document.getElementById('loc_addr_line1').value = addressSubstrings[0];
            document.getElementById('loc_city').value = addressSubstrings[1];
            document.getElementById('loc_state_code').value = addressSubstrings[2];
            document.getElementById('loc_zip_code').value = addressSubstrings[3];
        }

        // Determine assessment completions
        if (location.cope_date) {
            let cope_datef = new Date(location.cope_date).toISOString().slice(0, 10);
            document.getElementById('cope_date').value = cope_datef;
            document.getElementById('cope_eval').checked = location.cope_eval;

        }
        if (location.site_date) {
            let site_datef = new Date(location.site_date).toISOString().slice(0, 10);
            document.getElementById('site_date').value = site_datef;
            document.getElementById('site_eval').checked = location.site_eval;
        }

        // Fill input fields
        document.getElementById('location_id').value = location.location_id;
        document.getElementById('loc_customer_id').value = location.customer_id;

        // Open modal form with information entered
        locationModal.classList.add('active');

        // Apply edit-only properties
        locationInputForm.setAttribute('method', 'PUT');
        locationInputForm.setAttribute('onsubmit', 'editLocation()');
        locationEditButton.setAttribute('value', 'Edit Location');
        document.getElementById('location-title').textContent = 'Edit Location';

    } catch (error) { // Catch error
        console.error(`Error: ${error}`);
    }
}

function closeLocationForm() {
    locationModal.classList.remove('active');
    document.getElementById('addLocationForm').reset();
    locationId.innerHTML = '';

    // Remove edit-only properties
    locationInputForm.setAttribute('method', 'POST');
    locationInputForm.setAttribute('onsubmit', 'addLocation()');
    locationEditButton.setAttribute('value', 'Add Location');
    document.getElementById('location-title').textContent = 'Add Location';
}

/* ADD NEW LOCATION */
async function addLocation() {

    let add_address = '';

    if (document.getElementById('loc_addr_line2').value) {
        add_address = document.getElementById('loc_addr_line1').value + ', ' +
            document.getElementById('loc_addr_line2').value + ', ' +
            document.getElementById('loc_city').value + ', ' +
            document.getElementById('loc_state_code').value + ', ' +
            document.getElementById('loc_zip_code').value;
    }
    else {
        add_address = document.getElementById('loc_addr_line1').value + ', ' +
            document.getElementById('loc_city').value + ', ' +
            document.getElementById('loc_state_code').value + ', ' +
            document.getElementById('loc_zip_code').value;
    }

    console.log('Add Address:', add_address);

    const query = add_address.replace(/\s+/g, "+");

    try {
        const geoResponse = await fetch(`https://maps.googleapis.com/maps/api/geocode/json?address=${query}&key=***`);

        // Return error if response fails
        if (!geoResponse.ok) {
            const backError = await geoResponse.text();
            alert(backError);
            alert('Unable to get Geocode.');
            throw new Error(backError);
        }

        // Assign JSON object in a variable
        const geoData = await geoResponse.json();

        const loc = geoData.results[0].geometry.location;
        const lat = loc.lat;
        const lng = loc.lng;

        const location = {
            address: add_address,
            latitude: lat,
            longitude: lng,
            cope_eval: document.getElementById('cope_eval').checked,
            site_eval: document.getElementById('site_eval').checked,
            cope_date: document.getElementById('cope_date').value,
            site_date: document.getElementById('site_date').value,
            customer_id: Number(document.getElementById('loc_customer_id').value)
        };

        console.log('Submission:', JSON.stringify(location));

        fetch('https://localhost:7288/api/maps/add', {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(location)
        })
            .then(async response => {
                if (!response.ok) {
                    const backError = await response.text();
                    alert(backError);
                    alert('Unable to add location. ');
                    throw new Error(backError);
                }
                return response.json();
            })
            .then(async form_data => {
                alert('New location ID: ' + form_data.location_id);
                closeLocationForm();
                await getLocationDynamic(form_data.location_id);
                hasArrayElements(location_markers);
            })
            .catch(error => {
                console.error('Unable to add location.', error);
            })
    } catch (error) {
        console.error(`Error: ${error}`);
    }
}

/* EDIT LOCATION */
async function editLocation() {
    const location_id = document.getElementById('location_id').value;
    let add_address = '';

    if (document.getElementById('loc_addr_line2').value) {
        add_address = document.getElementById('loc_addr_line1').value + ', ' +
            document.getElementById('loc_addr_line2').value + ', ' +
            document.getElementById('loc_city').value + ', ' +
            document.getElementById('loc_state_code').value + ', ' +
            document.getElementById('loc_zip_code').value;
    }
    else {
        add_address = document.getElementById('loc_addr_line1').value + ', ' +
            document.getElementById('loc_city').value + ', ' +
            document.getElementById('loc_state_code').value + ', ' +
            document.getElementById('loc_zip_code').value;
    }

    console.log('Add Address:', add_address);

    const query = add_address.replace(/\s+/g, "+");

    try {
        const geoResponse = await fetch(`https://maps.googleapis.com/maps/api/geocode/json?address=${query}&key=AIzaSyBYVdWjc4-0m102zHkgWLe_SPfGHdso7rI`);

        // Return error if response fails
        if (!geoResponse.ok) {
            const backError = await geoResponse.text();
            alert(backError);
            alert('Unable to get Geocode.');
            throw new Error(backError);
        }

        // Assign JSON object in a variable
        const geoData = await geoResponse.json();

        const loc = geoData.results[0].geometry.location;
        const lat = loc.lat;
        const lng = loc.lng;

        const location = {
            address: add_address,
            latitude: lat,
            longitude: lng,
            cope_eval: document.getElementById('cope_eval').checked,
            site_eval: document.getElementById('site_eval').checked,
            cope_date: document.getElementById('cope_date').value,
            site_date: document.getElementById('site_date').value,
            customer_id: Number(document.getElementById('loc_customer_id').value)
        };

        console.log('Submission:', JSON.stringify(location));

        fetch(`https://localhost:7288/api/maps/${location_id}`, {
            method: 'PUT',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(location)
        })
            .then(async response => {
                if (!response.ok) {
                    const backError = await response.text();
                    alert(backError);
                    alert('Unable to add location. ');
                    throw new Error(backError);
                }
                return response.json();
            })
            .then(async form_data => {
                alert('Edited location ID: ' + form_data.location_id);
                closeLocationForm();
                
                // Requery for location list
                await searchBarLocation(form_data.address);
            })
            .catch(error => {
                console.error('Unable to add location.', error);
            })
    } catch (error) {
        console.error(`Error: ${error}`);
    }
}

/* GET LOCATION BY ID */
async function getLocation(location_id) {
    try {
        const response = await fetch(`https://localhost:7288/api/maps/${location_id}`);
        // Return error if response fails
        if (!response.ok) {
            const backError = await response.text();
            alert(backError);
            alert('Unable to get location.');
            throw new Error(backError);
        }

        // Assign JSON object in a variable
        const location = await response.json();

        // Return error if response is empty
        if (location.length == 0) {
            console.error('Location does not exist in database.');
        }

        // Update COPE Assessment trackers
        let cope_status = location.cope_eval;
        let cope_date = '';
        if (!location.cope_date) {
            cope_date = 'N/A';
        }
        else {
            cope_date = new Date(location.cope_date + 'T12:00:00').toLocaleDateString('en-US');
        }

        let cope_html = '';
        if (cope_status == true) {
            cope_html = `
             <div class="status-item completed">
             <svg width="20" height="20" viewBox="0 0 24 24" fill="transparent">
             <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
             <polyline points="22 4 12 14.01 9 11.01"></polyline>
             </svg>
             <div>
             <strong>COPE Supplement Complete</strong>
             <p>Date of last completion:<br>${cope_date}</p>
             </div>
             </div>
            `
        }
        else {
            cope_html = `
             <div class="status-item incomplete">
             <svg width="20" height="20" viewBox="0 0 24 24" fill="transparent">
             <circle cx="12" cy="12" r="10"></circle>
             <line x1="15" y1="9" x2="9" y2="15"></line>
             <line x1="9" y1="9" x2="15" y2="15"></line>
             </svg>
             <div>
             <strong>COPE Supplement Incomplete</strong>
             <p>Date of last completion:<br>${cope_date}</p>
             </div>
             </div>
            `
        }

        // Update On-Site Assessment trackers
        let site_status = location.site_eval;
        let site_date = '';
        if (!location.site_date) {
            site_date = 'N/A';
        }
        else {
            site_date = new Date(location.site_date + 'T12:00:00').toLocaleDateString('en-US');
        }

        let site_html = '';
        if (site_status == true) {
            site_html = `
             <div class="status-item completed">
             <svg width="20" height="20" viewBox="0 0 24 24" fill="transparent">
             <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
             <polyline points="22 4 12 14.01 9 11.01"></polyline>
             </svg>
             <div>
             <strong>On-Site Assessment Complete</strong>
             <p>Date of last completion:<br>${site_date}</p>
             </div>
             </div>
            `
        }
        else {
            site_html = `
             <div class="status-item incomplete">
             <svg width="20" height="20" viewBox="0 0 24 24" fill="transparent">
             <circle cx="12" cy="12" r="10"></circle>
             <line x1="15" y1="9" x2="9" y2="15"></line>
             <line x1="9" y1="9" x2="15" y2="15"></line>
             </svg>
             <div>
             <strong>On-Site Assessment Incomplete</strong>
             <p>Date of last completion:<br>${site_date}</p>
             </div>
             </div>
            `
        }
        let assessments = [];
        assessments.push(cope_html);
        assessments.push(site_html);
        return assessments;

    } catch (error) { // Catch error
        console.error(`Error: ${error}`);
    }
}

/* GET LOCATION DYNAMIC */
async function getLocationDynamic(location_id) {
    try {
        const response = await fetch(`https://localhost:7288/api/maps/${location_id}`);
        // Return error if response fails
        if (!response.ok) {
            const backError = await response.text();
            alert(backError);
            alert('Unable to get location.');
            throw new Error(backError);
        }

        // Assign JSON object in a variable
        const location = await response.json();

        addMarkersToMap([location]);

        // Return error if response is empty
        if (location.length == 0) {
            console.error('Location does not exist in database.');
        }

        // Update COPE Assessment trackers
        let cope_status = location.cope_eval;
        let cope_date = '';
        if (!location.cope_date) {
            cope_date = 'N/A';
        }
        else {
            cope_date = new Date(location.cope_date + 'T12:00:00').toLocaleDateString('en-US');
        }

        let cope_html = '';
        if (cope_status == true) {
            cope_html = `
             <div class="status-item completed">
             <svg width="20" height="20" viewBox="0 0 24 24" fill="transparent">
             <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
             <polyline points="22 4 12 14.01 9 11.01"></polyline>
             </svg>
             <div>
             <strong>COPE Supplement Complete</strong>
             <p>Date of last completion:<br>${cope_date}</p>
             </div>
             </div>
            `
        }
        else {
            cope_html = `
             <div class="status-item incomplete">
             <svg width="20" height="20" viewBox="0 0 24 24" fill="transparent">
             <circle cx="12" cy="12" r="10"></circle>
             <line x1="15" y1="9" x2="9" y2="15"></line>
             <line x1="9" y1="9" x2="15" y2="15"></line>
             </svg>
             <div>
             <strong>COPE Supplement Incomplete</strong>
             <p>Date of last completion:<br>${cope_date}</p>
             </div>
             </div>
            `
        }

        // Update On-Site Assessment trackers
        let site_status = location.site_eval;
        let site_date = '';
        if (!location.site_date) {
            site_date = 'N/A';
        }
        else {
            site_date = new Date(location.site_date + 'T12:00:00').toLocaleDateString('en-US');
        }

        let site_html = '';
        if (site_status == true) {
            site_html = `
             <div class="status-item completed">
             <svg width="20" height="20" viewBox="0 0 24 24" fill="transparent">
             <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
             <polyline points="22 4 12 14.01 9 11.01"></polyline>
             </svg>
             <div>
             <strong>On-Site Assessment Complete</strong>
             <p>Date of last completion:<br>${site_date}</p>
             </div>
             </div>
            `
        }
        else {
            site_html = `
             <div class="status-item incomplete">
             <svg width="20" height="20" viewBox="0 0 24 24" fill="transparent">
             <circle cx="12" cy="12" r="10"></circle>
             <line x1="15" y1="9" x2="9" y2="15"></line>
             <line x1="9" y1="9" x2="15" y2="15"></line>
             </svg>
             <div>
             <strong>On-Site Assessment Incomplete</strong>
             <p>Date of last completion:<br>${site_date}</p>
             </div>
             </div>
            `
        }

        const table = document.querySelector('#location-list');
        // Get COPE & On-Site assessments from Maps API

        const row = document.createElement('div');
        row.className = 'location-card'; // Assign class="location-card" to created elements for CSS formatting
        row.innerHTML = `
            <div class="location-info">
            <p>${location.address}</p>
            <a href="../Location/customer-location.html?customer_id=${location.customer_id}&location_id=${location.location_id}" class="view-link"><button class="openModalBtn">View More Details</button></a>
            <button class="openModalBtn" onclick="openLocationEditForm(${location.location_id})">Edit</button>
            </div>
             <div class="status-grid">
             ${cope_html}
             ${site_html}
             </div>
            `;

        row.addEventListener("click", () => {
            activateCard(row);
            activateMarker(location.location_id);
        });
        // Add to table
        table.appendChild(row);

    } catch (error) { // Catch error
        console.error(`Error: ${error}`);
    }
}

// Handles user input for geo search
async function searchBarLocation(address_input) {
    if (hasArrayElements(location_markers))
        clearLocationMarkers();
    try {
        const georesult = await geocodeLocation(address_input);
        const location = georesult.geometry.location;
        const loc_types = georesult.types;
        const bounds = georesult.geometry.viewport;

        const search_type = isRegion(loc_types) ? "region" : "proximity";

        const request = {
            search_type: search_type,
            latitude: location.lat(),
            longitude: location.lng(),
        };

        // Extracts viewport w/ bounding box if available
        if (search_type === "region") {
            if (bounds && bounds.getNorthEast && bounds.getSouthWest) {
                const ne = bounds.getNorthEast();
                const sw = bounds.getSouthWest();

                request.min_latitude = sw.lat();
                request.max_latitude = ne.lat();
                request.min_longitude = sw.lng();
                request.max_longitude = ne.lng();
            }
        }

        // Updates map to reflect location query
        map.panTo(location);
        setZoomLevel(loc_types);

        //console.log("HERE***: ", request);

        // Queries to backend for a response
        const response = await fetch("https://localhost:7288/api/maps/search", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(request)
        });

        if (!response.ok) {
            throw new Error("HTTP request failed: ", response.status);
        }

        const nearbyLocations = await response.json();
        clearLocationMarkers();

        // Checks to see if response has any locations
        if (hasArrayElements(nearbyLocations)) {
            await addLocationsToList(nearbyLocations);
            addMarkersToMap(nearbyLocations);
        } else {
            loc_list.innerHTML = "No locations found";
        }

    } catch (error) {
        console.error("ERROR:", error);
    }
}


/* ONLOAD */
window.onload = async () => {
    const customer_id = await getCustomers();
    if (!customer_id) return;
};
