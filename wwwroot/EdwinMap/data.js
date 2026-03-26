const locationsData = [
    {
        id: 1,
        name: '180 N Michigan Avenue',
        address: '180 N Michigan Avenue Suite 1210',
        city: 'Chicago',
        state: 'IL',
        zip: '60601',
        country: 'United States',
        cofsCompleted: true,
        cofsDate: '10/20/25',
        assessmentCompleted: true,
        assessmentDate: '10/20/25',
        assessmentInfo: '',
        activePolicies: 5
    },
    {
        id: 2,
        name: '180 N Michigan Avenue',
        address: '180 N Michigan Avenue Suite 1210',
        city: 'Chicago',
        state: 'IL',
        zip: '60601',
        country: 'United States',
        cofsCompleted: true,
        cofsDate: '07/15/25',
        assessmentCompleted: true,
        assessmentDate: '07/15/25',
        assessmentInfo: '',
        activePolicies: 5
    },
    {
        id: 3,
        name: '250 W 34th Street',
        address: '205 W 34th Street Suite 501',
        city: 'New York',
        state: 'NY',
        zip: '10001',
        country: 'United States',
        cofsCompleted: true,
        cofsDate: '05/23/25',
        assessmentCompleted: true,
        assessmentDate: '05/23/25',
        assessmentInfo: '',
        activePolicies: 5
    },
    {
        id: 4,
        name: '180 N Michigan Avenue',
        address: '180 N Michigan Avenue Suite 1210',
        city: 'Chicago',
        state: 'IL',
        zip: '60601',
        country: 'United States',
        cofsCompleted: true,
        cofsDate: '11/01/25',
        assessmentCompleted: false,
        assessmentDate: '',
        assessmentInfo: 'On-site assessment info not available',
        activePolicies: 5
    },
    {
        id: 5,
        name: '500 S Wacker Drive',
        address: '500 S Wacker Drive Suite 300',
        city: 'Chicago',
        state: 'IL',
        zip: '60606',
        country: 'United States',
        cofsCompleted: true,
        cofsDate: '06/17/25',
        assessmentCompleted: true,
        assessmentDate: '06/17/25',
        assessmentInfo: '',
        activePolicies: 3
    },
    {
        id: 6,
        name: '350 N Orleans Street',
        address: '350 N Orleans Street Floor 2',
        city: 'Chicago',
        state: 'IL',
        zip: '60654',
        country: 'United States',
        cofsCompleted: false,
        cofsDate: '',
        assessmentCompleted: false,
        assessmentDate: '',
        assessmentInfo: 'Pending assessment',
        activePolicies: 7
    },
    {
        id: 7,
        name: '233 S Wacker Drive',
        address: '233 S Wacker Drive Suite 8400',
        city: 'Chicago',
        state: 'IL',
        zip: '60606',
        country: 'United States',
        cofsCompleted: true,
        cofsDate: '09/08/25',
        assessmentCompleted: true,
        assessmentDate: '09/18/25',
        assessmentInfo: '',
        activePolicies: 4
    },
    {
        id: 8,
        name: '150 N Riverside Plaza',
        address: '150 N Riverside Plaza Suite 5200',
        city: 'Chicago',
        state: 'IL',
        zip: '60606',
        country: 'United States',
        cofsCompleted: true,
        cofsDate: '04/24/25',
        assessmentCompleted: false,
        assessmentDate: '',
        assessmentInfo: 'Assessment scheduled for next month',
        activePolicies: 2
    }
];


function getAllLocations() {
    return locationsData;
}


function getLocationById(id) {
    return locationsData.find(location => location.id === id);
}

function addLocation(location) {
    const newId = Math.max(...locationsData.map(l => l.id)) + 1;
    const newLocation = {
        id: newId,
        ...location,
        cofsCompleted: false,
        cofsDate: '',
        assessmentCompleted: false,
        assessmentDate: '',
        assessmentInfo: 'Pending'
    };
    locationsData.push(newLocation);
    return newLocation;
}

function updateLocation(id, updatedData) {
    const index = locationsData.findIndex(location => location.id === id);
    if (index !== -1) {
        locationsData[index] = { ...locationsData[index], ...updatedData };
        return locationsData[index];
    }
    return null;
}

function deleteLocation(id) {
    const index = locationsData.findIndex(location => location.id === id);
    if (index !== -1) {
        locationsData.splice(index, 1);
        return true;
    }
    return false;
}


function searchLocations(searchTerm) {
    const term = searchTerm.toLowerCase();
    return locationsData.filter(location => 
        location.name.toLowerCase().includes(term) ||
        location.address.toLowerCase().includes(term) ||
        location.city.toLowerCase().includes(term) ||
        location.state.toLowerCase().includes(term)
    );
}


function filterLocationsByStatus(cofsStatus, assessmentStatus) {
    return locationsData.filter(location => {
        const cofsMatch = cofsStatus === null || location.cofsCompleted === cofsStatus;
        const assessmentMatch = assessmentStatus === null || location.assessmentCompleted === assessmentStatus;
        return cofsMatch && assessmentMatch;
    });
}


function sortLocations(sortBy, order = 'asc') {
    const sorted = [...locationsData].sort((a, b) => {
        let aValue, bValue;
        
        switch(sortBy) {
            case 'name':
                aValue = a.name.toLowerCase();
                bValue = b.name.toLowerCase();
                break;
            case 'city':
                aValue = a.city.toLowerCase();
                bValue = b.city.toLowerCase();
                break;
            case 'sites':
                aValue = a.activePolicies;
                bValue = b.activePolicies;
                break;
            case 'date':
                aValue = new Date(a.cofsDate);
                bValue = new Date(b.cofsDate);
                break;
            default:
                return 0;
        }
        
        if (aValue < bValue) return order === 'asc' ? -1 : 1;
        if (aValue > bValue) return order === 'asc' ? 1 : -1;
        return 0;
    });
    
    return sorted;
}