const express = require('express');
const cors = require('cors');
const bodyParser = require('body-parser');
const path = require('path');

const app = express();
const PORT = process.env.PORT || 3000;

app.use(cors());
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));


app.use(express.static(path.join(__dirname, 'public')));

let locations = [
    {
        id: 1,
        name: '180 N Michigan Avenue',
        address: '180 N Michigan Avenue Suite 1210',
        city: 'Chicago',
        state: 'IL',
        zip: '60601',
        country: 'United States',
        cofsCompleted: true,
        cofsDate: '07/05/25',
        assessmentCompleted: true,
        assessmentDate: '07/05/25',
        assessmentInfo: '',
        activePolicies: 5
    },
    {
        id: 2,
        name: '250 W 34th Street',
        address: '205 W 34th Street Suite 501',
        city: 'New York',
        state: 'NY',
        zip: '10001',
        country: 'United States',
        cofsCompleted: true,
        cofsDate: '07/05/25',
        assessmentCompleted: true,
        assessmentDate: '07/05/25',
        assessmentInfo: '',
        activePolicies: 5
    }
];

app.get('/api/locations', (req, res) => {
    const { search, state, cofsCompleted, assessmentCompleted, sortBy, order } = req.query;
    
    let filteredLocations = [...locations];
    
    if (search) {
        const searchLower = search.toLowerCase();
        filteredLocations = filteredLocations.filter(loc => 
            loc.name.toLowerCase().includes(searchLower) ||
            loc.address.toLowerCase().includes(searchLower) ||
            loc.city.toLowerCase().includes(searchLower)
        );
    }
    
    if (state) {
        filteredLocations = filteredLocations.filter(loc => loc.state === state);
    }
    

    if (cofsCompleted !== undefined) {
        filteredLocations = filteredLocations.filter(loc => 
            loc.cofsCompleted === (cofsCompleted === 'true')
        );
    }
    
    if (assessmentCompleted !== undefined) {
        filteredLocations = filteredLocations.filter(loc => 
            loc.assessmentCompleted === (assessmentCompleted === 'true')
        );
    }
    
    if (sortBy) {
        filteredLocations.sort((a, b) => {
            let aVal = a[sortBy];
            let bVal = b[sortBy];
            
            if (typeof aVal === 'string') {
                aVal = aVal.toLowerCase();
                bVal = bVal.toLowerCase();
            }
            
            if (order === 'desc') {
                return aVal < bVal ? 1 : aVal > bVal ? -1 : 0;
            }
            return aVal < bVal ? -1 : aVal > bVal ? 1 : 0;
        });
    }
    
    res.json({
        success: true,
        data: filteredLocations,
        count: filteredLocations.length
    });
});

app.get('/api/locations/:id', (req, res) => {
    const id = parseInt(req.params.id);
    const location = locations.find(loc => loc.id === id);
    
    if (!location) {
        return res.status(404).json({
            success: false,
            message: 'Location not found'
        });
    }
    
    res.json({
        success: true,
        data: location
    });
});


app.post('/api/locations', (req, res) => {
    const { name, address, city, state, zip, country, activePolicies } = req.body;
    
    if (!name || !address || !city || !state || !zip || !country) {
        return res.status(400).json({
            success: false,
            message: 'Missing required fields'
        });
    }
    
    const newId = locations.length > 0 ? Math.max(...locations.map(l => l.id)) + 1 : 1;
    
    const newLocation = {
        id: newId,
        name,
        address,
        city,
        state,
        zip,
        country,
        cofsCompleted: false,
        cofsDate: '',
        assessmentCompleted: false,
        assessmentDate: '',
        assessmentInfo: 'Pending',
        activePolicies: activePolicies || 1
    };
    
    locations.push(newLocation);
    
    res.status(201).json({
        success: true,
        message: 'Location created successfully',
        data: newLocation
    });
});

app.put('/api/locations/:id', (req, res) => {
    const id = parseInt(req.params.id);
    const locationIndex = locations.findIndex(loc => loc.id === id);
    
    if (locationIndex === -1) {
        return res.status(404).json({
            success: false,
            message: 'Location not found'
        });
    }
    
    const updatedLocation = {
        ...locations[locationIndex],
        ...req.body,
        id 
    };
    
    locations[locationIndex] = updatedLocation;
    
    res.json({
        success: true,
        message: 'Location updated successfully',
        data: updatedLocation
    });
});

app.patch('/api/locations/:id/status', (req, res) => {
    const id = parseInt(req.params.id);
    const { cofsCompleted, cofsDate, assessmentCompleted, assessmentDate, assessmentInfo } = req.body;
    
    const locationIndex = locations.findIndex(loc => loc.id === id);
    
    if (locationIndex === -1) {
        return res.status(404).json({
            success: false,
            message: 'Location not found'
        });
    }
    
    if (cofsCompleted !== undefined) {
        locations[locationIndex].cofsCompleted = cofsCompleted;
        locations[locationIndex].cofsDate = cofsDate || '';
    }
    
    if (assessmentCompleted !== undefined) {
        locations[locationIndex].assessmentCompleted = assessmentCompleted;
        locations[locationIndex].assessmentDate = assessmentDate || '';
        locations[locationIndex].assessmentInfo = assessmentInfo || '';
    }
    
    res.json({
        success: true,
        message: 'Location status updated successfully',
        data: locations[locationIndex]
    });
});

app.delete('/api/locations/:id', (req, res) => {
    const id = parseInt(req.params.id);
    const locationIndex = locations.findIndex(loc => loc.id === id);
    
    if (locationIndex === -1) {
        return res.status(404).json({
            success: false,
            message: 'Location not found'
        });
    }
    
    const deletedLocation = locations.splice(locationIndex, 1)[0];
    
    res.json({
        success: true,
        message: 'Location deleted successfully',
        data: deletedLocation
    });
});

app.get('/api/stats', (req, res) => {
    const totalLocations = locations.length;
    const cofsCompleted = locations.filter(loc => loc.cofsCompleted).length;
    const assessmentCompleted = locations.filter(loc => loc.assessmentCompleted).length;
    const totalSites = locations.reduce((sum, loc) => sum + loc.activePolicies, 0);
    
    const stateDistribution = locations.reduce((acc, loc) => {
        acc[loc.state] = (acc[loc.state] || 0) + 1;
        return acc;
    }, {});
    
    res.json({
        success: true,
        data: {
            totalLocations,
            cofsCompleted,
            cofsCompletionRate: totalLocations > 0 ? (cofsCompleted / totalLocations * 100).toFixed(2) : 0,
            assessmentCompleted,
            assessmentCompletionRate: totalLocations > 0 ? (assessmentCompleted / totalLocations * 100).toFixed(2) : 0,
            totalSites,
            stateDistribution
        }
    });
});

app.use((err, req, res, next) => {
    console.error(err.stack);
    res.status(500).json({
        success: false,
        message: 'Internal server error',
        error: process.env.NODE_ENV === 'development' ? err.message : undefined
    });
});

app.use((req, res) => {
    res.status(404).json({
        success: false,
        message: 'Route not found'
    });
});

app.listen(PORT, () => {
    console.log(`Server is running on port ${PORT}`);
    console.log(`API available at http://localhost:${PORT}/api`);
});