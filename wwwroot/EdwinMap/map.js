
function initMap() {
    
    var options = {
        zoom: 6,
        center: { lat: 40.6331, lng: -89.3985 } 
    };

    var map = new google.maps.Map(document.getElementById('map'), options);

    google.maps.event.addListener(map, 'click', function (event) {
        addMarker({ coords: event.latLng });
    });
    
    //not using predefined markers
    // Array of predefined markers
    var markers = [
        {
            coords: { lat: 41.88452, lng: -87.62485 },
            content: '<h3>Chicago</h3>'
        },
        {
            coords: { lat: 40.75117, lng: -73.99154 },
            content: '<h3>New York</h3>'
        },
        {
            coords: { lat: 37.79166, lng: -122.39442 },
            content: '<h3>San Francisco</h3>'
        },
        {
            coords: { lat: 30.27973, lng: -97.75815 },
            content: '<h3>Austin</h3>'
        },
        {
            coords: { lat: 45.50703, lng: -122.67294 },
            content: '<h3>Portland</h3>'
        },
        {
            coords: { lat: 39.71136, lng: -104.93625 },
            content: '<h3>Denver</h3>'
        },
        {
            coords: { lat: 36.18174, lng: -115.17351 },
            content: '<h3>Las Vegas</h3>'
        }
    ];
    
    // Loop through markers array and add them to the map
    for (var i = 0; i < markers.length; i++) {
        addMarker(markers[i]);
    }

    function addMarker(props) {
        var marker = new google.maps.Marker({
            position: props.coords,
            map: map
        });

        if (props.content) {
            var infoWindow = new google.maps.InfoWindow({
                content: props.content
            });

            marker.addListener('click', function() {
                infoWindow.open(map, marker);
            });
        }
    }
}


document.addEventListener('DOMContentLoaded', function() {
    console.log('Completed and ready');
    
});

