let currentLocations = getAllLocations();
let currentSearchTerm = '';
let currentView = 'list';
let displayedCount = 4;
const loadIncrement = 4;

const searchInput = document.getElementById('searchInput');
const locationsContainer = document.getElementById('locationsContainer');
const loadMoreBtn = document.getElementById('loadMoreBtn');
const viewToggle = document.getElementById('viewToggle');
const addLocationBtn = document.getElementById('addLocationBtn');
const locationModal = document.getElementById('locationModal');
const addLocationModal = document.getElementById('addLocationModal');
const modalClose = document.getElementById('modalClose');
const addModalClose = document.getElementById('addModalClose');
const modalCloseBtn = document.getElementById('modalCloseBtn');
const addModalCancelBtn = document.getElementById('addModalCancelBtn');
const addModalSaveBtn = document.getElementById('addModalSaveBtn');
const addLocationForm = document.getElementById('addLocationForm');

document.addEventListener('DOMContentLoaded', function() {
    renderLocations();
    setupEventListeners();
});


function setupEventListeners() {

    searchInput.addEventListener('input', handleSearch);
    

    viewToggle.addEventListener('click', handleViewToggle);
    

    loadMoreBtn.addEventListener('click', handleLoadMore);

    addLocationBtn.addEventListener('click', () => openModal(addLocationModal));
    addModalClose.addEventListener('click', () => closeModal(addLocationModal));
    addModalCancelBtn.addEventListener('click', () => closeModal(addLocationModal));
    addModalSaveBtn.addEventListener('click', handleAddLocation);
    
    modalClose.addEventListener('click', () => closeModal(locationModal));
    modalCloseBtn.addEventListener('click', () => closeModal(locationModal));
    
    locationModal.addEventListener('click', (e) => {
        if (e.target === locationModal) {
            closeModal(locationModal);
        }
    });
    
    addLocationModal.addEventListener('click', (e) => {
        if (e.target === addLocationModal) {
            closeModal(addLocationModal);
        }
    });
}

function handleSearch(e) {
    currentSearchTerm = e.target.value;
    displayedCount = loadIncrement;
    
    if (currentSearchTerm.length === 0) {
        currentLocations = getAllLocations();
    } else {
        currentLocations = searchLocations(currentSearchTerm);
    }
    
    renderLocations();
}

function handleViewToggle() {
    currentView = currentView === 'list' ? 'grid' : 'list';
    const icon = viewToggle.querySelector('i');
    
    if (currentView === 'grid') {
        icon.className = 'fas fa-list';
        locationsContainer.style.display = 'grid';
        locationsContainer.style.gridTemplateColumns = 'repeat(auto-fill, minmax(400px, 1fr))';
        locationsContainer.style.gap = '1rem';
    } else {
        icon.className = 'fas fa-th';
        locationsContainer.style.display = 'flex';
        locationsContainer.style.flexDirection = 'column';
        locationsContainer.style.gap = '1rem';
    }
}

function handleLoadMore() {
    displayedCount += loadIncrement;
    renderLocations();
}

function renderLocations() {
    locationsContainer.innerHTML = '';
    
    const locationsToShow = currentLocations.slice(0, displayedCount);
    
    if (locationsToShow.length === 0) {
        locationsContainer.innerHTML = `
            <div style="text-align: center; padding: 3rem; color: #6b7280;">
                <i class="fas fa-search" style="font-size: 3rem; margin-bottom: 1rem;"></i>
                <p>No locations found matching your search.</p>
            </div>
        `;
        loadMoreBtn.style.display = 'none';
        return;
    }
    
    locationsToShow.forEach(location => {
        const card = createLocationCard(location);
        locationsContainer.appendChild(card);
    });
    
    if (displayedCount >= currentLocations.length) {
        loadMoreBtn.style.display = 'none';
    } else {
        loadMoreBtn.style.display = 'block';
    }
}

function createLocationCard(location) {
    const card = document.createElement('div');
    card.className = 'location-card';
    
    card.innerHTML = `
        <div class="location-card-content">
            <div class="location-info">
                <h3 class="location-name">${location.name}</h3>
                <p class="location-address">${location.address}</p>
                <p class="location-address">${location.city}, ${location.state} ${location.zip}, ${location.country}</p>
                
                <div class="location-status-grid">
                    <div class="status-item">
                        <i class="fas ${location.cofsCompleted ? 'fa-check-circle' : 'fa-times-circle'} status-icon ${location.cofsCompleted ? 'completed' : 'incomplete'}"></i>
                        <div class="status-details">
                            <h4>COFS Supplement ${location.cofsCompleted ? 'Completed' : 'Not Completed'}</h4>
                            ${location.cofsCompleted ? `
                                <p>Date of last completion:</p>
                                <p style="color: #111827;">${location.cofsDate}</p>
                            ` : '<p>Not yet completed</p>'}
                        </div>
                    </div>
                    
                    <div class="status-item">
                        <i class="fas ${location.assessmentCompleted ? 'fa-check-circle' : 'fa-times-circle'} status-icon ${location.assessmentCompleted ? 'completed' : 'incomplete'}"></i>
                        <div class="status-details">
                            <h4>${location.assessmentCompleted ? 'On-site Assessment Completed' : 'On-site Assessment Not Completed'}</h4>
                            ${location.assessmentCompleted ? `
                                <p>Date of last completion:</p>
                                <p style="color: #111827;">${location.assessmentDate}</p>
                            ` : `<p>${location.assessmentInfo}</p>`}
                        </div>
                    </div>
                </div>
                
                <button class="view-details-btn" onclick="viewLocationDetails(${location.id})">
                    View more details →
                </button>
            </div>
            
            <div class="location-sites">
                <div class="sites-badge">
                    <span class="sites-number">${location.activePolicies}</span>
                    <span class="sites-label">ACTIVE</span>
                    <span class="sites-label">POLICIES</span>
                </div>
            </div>
        </div>
    `;
    
    return card;
}


function viewLocationDetails(locationId) {
    const location = getLocationById(locationId);
    if (!location) return;
    
    const modalBody = document.getElementById('modalBody');
    const modalTitle = document.getElementById('modalTitle');
    
    modalTitle.textContent = location.name;
    
    modalBody.innerHTML = `
        <div style="margin-bottom: 1.5rem;">
            <p style="font-size: 0.875rem; color: #6b7280; margin-bottom: 0.25rem;">Address</p>
            <p style="font-weight: 500;">${location.address}</p>
            <p style="font-weight: 500;">${location.city}, ${location.state} ${location.zip}</p>
            <p style="font-weight: 500;">${location.country}</p>
        </div>
        
        <div style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 1rem; margin-bottom: 1.5rem;">
            <div>
                <p style="font-size: 0.875rem; color: #6b7280; margin-bottom: 0.25rem;">COFS Status</p>
                <p style="font-weight: 500;">${location.cofsCompleted ? 'Completed' : 'Not Completed'}</p>
                <p style="font-size: 0.875rem; color: #6b7280;">${location.cofsDate || 'N/A'}</p>
            </div>
            <div>
                <p style="font-size: 0.875rem; color: #6b7280; margin-bottom: 0.25rem;">Assessment Status</p>
                <p style="font-weight: 500;">${location.assessmentCompleted ? 'Completed' : 'Not Completed'}</p>
                <p style="font-size: 0.875rem; color: #6b7280;">${location.assessmentDate || location.assessmentInfo}</p>
            </div>
        </div>
        
        <div>
            <p style="font-size: 0.875rem; color: #6b7280; margin-bottom: 0.25rem;">Active Policies</p>
            <p style="font-size: 1.5rem; font-weight: bold;">${location.activePolicies}</p>
        </div>
    `;
    
    openModal(locationModal);
}


function handleAddLocation(e) {
    e.preventDefault();
    
    const formData = {
        name: document.getElementById('locationName').value,
        address: document.getElementById('address').value,
        city: document.getElementById('city').value,
        state: document.getElementById('state').value,
        zip: document.getElementById('zip').value,
        country: document.getElementById('country').value,
        activePolicies: parseInt(document.getElementById('activePolicies').value)
    };
    

    if (!formData.name || !formData.address || !formData.city || !formData.state || !formData.zip) {
        alert('Please fill in all required fields.');
        return;
    }
    

    const newLocation = addLocation(formData);
    

    currentLocations = getAllLocations();
    displayedCount = loadIncrement;
    renderLocations();
    

    addLocationForm.reset();
    closeModal(addLocationModal);
    
    showNotification('Location added successfully!');
}

function openModal(modal) {
    modal.classList.add('active');
    document.body.style.overflow = 'hidden';
}

function closeModal(modal) {
    modal.classList.remove('active');
    document.body.style.overflow = 'auto';
}

function showNotification(message) {
    const notification = document.createElement('div');
    notification.style.cssText = `
        position: fixed;
        top: 2rem;
        right: 2rem;
        background-color: #10b981;
        color: white;
        padding: 1rem 1.5rem;
        border-radius: 0.5rem;
        box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        z-index: 2000;
        animation: slideIn 0.3s ease-out;
    `;
    notification.textContent = message;
    
    document.body.appendChild(notification);
    
    setTimeout(() => {
        notification.style.animation = 'slideOut 0.3s ease-out';
        setTimeout(() => {
            document.body.removeChild(notification);
        }, 300);
    }, 3000);
}

const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOut {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(100%);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);