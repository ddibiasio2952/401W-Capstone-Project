/* DAN UPDATE 11/27/2025 AM */
/* TOPBAR SEARCH */
const navSearch = document.querySelector(".header-center input");
const searchButton = document.getElementById('search-btn');
const searchPlaceholder = document.getElementById('nav-search');
var searchFilter = document.getElementById('nav-select');
let locationArray = [];

// Get location list if Location Filter is selected
searchFilter.addEventListener('change', function () {
    if (this.value === 'State') {

    }
    else if (this.value === 'Location') {
        filterLocations()
    }
});

/* GET LOCATIONS FOR FILTER */
async function filterLocations() {
    try {
        const response = await fetch(`https://localhost:7288/api/maps/`);

        // Return error if response fails
        if (!response.ok) {
            const backError = await response.text();
            alert(backError);
            alert('Unable to get locations.');
            throw new Error(backError);
        }

        // Assign JSON object in a variable
        const locations = await response.json();
        // Empty location ID array for search filter to reference
        locationArray = [];
        for (const location of locations) {
            locationArray.push(location.location_id); // Add location to array
        };
        console.log('Location array populated with', locationArray.length, 'rows');

        // Save location array to session storage
        sessionStorage.setItem('locationArray', JSON.stringify(locationArray));

    } catch (error) { // Catch error
        console.error(`Error: ${error}`);
    }
}

// Handles user input for navbar search
// Keyboard listener
navSearch.addEventListener("keydown", async (event) => {
    if (event.key === "Enter") {
        const search_input = navSearch.value;
        // Call search function.
        enterSearch(search_input);
    }
});
// Search button listener
searchButton.addEventListener('click', function () {
    const search_input = navSearch.value;
    // Call search function.
    enterSearch(search_input);
});

/* ENTER SEARCH */
function enterSearch(search_input) {
    // Refuse empty input
    if (!search_input || search_input.trim() === "") {
        loc_list.innerHTML = "No locations found";
        alert("Invalid input.");

        if (hasArrayElements(location_markers))
            clearLocationMarkers();

        return;
    }

    // Validate input according to filter
    // State
    if (searchFilter && searchFilter.value === 'State') {
        // Catch state abbreviation
        if (search_input.length < 4) {
            alert('Enter a full state name.');
            return;
        }
        window.location.href = `https://localhost:7288/Map/client-search.html?searchstring=${search_input}`;
    }
    // Location
    if (searchFilter && searchFilter.value === 'Location') {
        var int_input = parseInt(search_input);
        if ((!isNaN(int_input) && search_input.trim() === int_input.toString()) && locationArray.includes(int_input)) {
            window.location.href = `https://localhost:7288/Location/customer-location.html?location_id=${int_input}`;
        }
        else {
            alert('Enter a valid Location ID.');
            return;
        }
    }
}