let currentFilters = {
    cofsCompleted: null,
    assessmentCompleted: null
};

let currentSort = {
    by: null,
    order: 'asc'
};

function initializeFiltersAndSort() {
    const filterBtn = document.getElementById('filterBtn');
    const sortBtn = document.getElementById('sortBtn');
    
    if (filterBtn) {
        filterBtn.addEventListener('click', toggleFilterDropdown);
    }
    
    if (sortBtn) {
        sortBtn.addEventListener('click', toggleSortDropdown);
    }
    
    document.addEventListener('click', (e) => {
        if (!e.target.closest('.btn-filter') && !e.target.closest('.filter-dropdown')) {
            closeFilterDropdown();
        }
        if (!e.target.closest('.btn-filter') && !e.target.closest('.sort-dropdown')) {
            closeSortDropdown();
        }
    });
}

function toggleFilterDropdown(e) {
    e.stopPropagation();
    closeSortDropdown();
    
    let dropdown = document.querySelector('.filter-dropdown');
    
    if (dropdown) {
        dropdown.remove();
    } else {
        createFilterDropdown();
    }
}

function createFilterDropdown() {
    const filterBtn = document.getElementById('filterBtn');
    const dropdown = document.createElement('div');
    dropdown.className = 'filter-dropdown';
    
    dropdown.innerHTML = `
        <div class="dropdown-header">
            <h4>Filter By</h4>
            <button class="clear-filters-btn" onclick="clearFilters()">Clear All</button>
        </div>
        
        <div class="filter-section">
            <h5>COFS Status</h5>
            <label class="filter-option">
                <input type="radio" name="cofs" value="all" ${currentFilters.cofsCompleted === null ? 'checked' : ''}>
                <span>All</span>
            </label>
            <label class="filter-option">
                <input type="radio" name="cofs" value="true" ${currentFilters.cofsCompleted === true ? 'checked' : ''}>
                <span>Completed</span>
            </label>
            <label class="filter-option">
                <input type="radio" name="cofs" value="false" ${currentFilters.cofsCompleted === false ? 'checked' : ''}>
                <span>Not Completed</span>
            </label>
        </div>
        
        <div class="filter-section">
            <h5>Assessment Status</h5>
            <label class="filter-option">
                <input type="radio" name="assessment" value="all" ${currentFilters.assessmentCompleted === null ? 'checked' : ''}>
                <span>All</span>
            </label>
            <label class="filter-option">
                <input type="radio" name="assessment" value="true" ${currentFilters.assessmentCompleted === true ? 'checked' : ''}>
                <span>Completed</span>
            </label>
            <label class="filter-option">
                <input type="radio" name="assessment" value="false" ${currentFilters.assessmentCompleted === false ? 'checked' : ''}>
                <span>Not Completed</span>
            </label>
        </div>
        
        <div class="dropdown-footer">
            <button class="btn-secondary" onclick="closeFilterDropdown()">Cancel</button>
            <button class="btn-primary" onclick="applyFilters()">Apply Filters</button>
        </div>
    `;
    
    filterBtn.parentElement.style.position = 'relative';
    filterBtn.parentElement.appendChild(dropdown);
    
    const cofsRadios = dropdown.querySelectorAll('input[name="cofs"]');
    const assessmentRadios = dropdown.querySelectorAll('input[name="assessment"]');
    
    cofsRadios.forEach(radio => {
        radio.addEventListener('change', updateFilterPreview);
    });
    
    assessmentRadios.forEach(radio => {
        radio.addEventListener('change', updateFilterPreview);
    });
}

function updateFilterPreview() {

}

function applyFilters() {
    const dropdown = document.querySelector('.filter-dropdown');
    if (!dropdown) return;
    
    const cofsValue = dropdown.querySelector('input[name="cofs"]:checked').value;
    const assessmentValue = dropdown.querySelector('input[name="assessment"]:checked').value;
    
    currentFilters.cofsCompleted = cofsValue === 'all' ? null : cofsValue === 'true';
    currentFilters.assessmentCompleted = assessmentValue === 'all' ? null : assessmentValue === 'true';
    
    filterAndRenderLocations();
    closeFilterDropdown();
    updateFilterButtonBadge();
}

function clearFilters() {
    currentFilters.cofsCompleted = null;
    currentFilters.assessmentCompleted = null;
    
    filterAndRenderLocations();
    closeFilterDropdown();
    updateFilterButtonBadge();
}

function closeFilterDropdown() {
    const dropdown = document.querySelector('.filter-dropdown');
    if (dropdown) {
        dropdown.remove();
    }
}

function updateFilterButtonBadge() {
    const filterBtn = document.getElementById('filterBtn');
    let activeFilters = 0;
    
    if (currentFilters.cofsCompleted !== null) activeFilters++;
    if (currentFilters.assessmentCompleted !== null) activeFilters++;
    
    const existingBadge = filterBtn.querySelector('.filter-badge');
    if (existingBadge) {
        existingBadge.remove();
    }
    
    if (activeFilters > 0) {
        const badge = document.createElement('span');
        badge.className = 'filter-badge';
        badge.textContent = activeFilters;
        filterBtn.appendChild(badge);
    }
}

function toggleSortDropdown(e) {
    e.stopPropagation();
    closeFilterDropdown();
    
    let dropdown = document.querySelector('.sort-dropdown');
    
    if (dropdown) {
        dropdown.remove();
    } else {
        createSortDropdown();
    }
}

function createSortDropdown() {
    const sortBtn = document.getElementById('sortBtn');
    const dropdown = document.createElement('div');
    dropdown.className = 'sort-dropdown';
    
    dropdown.innerHTML = `
        <div class="dropdown-header">
            <h4>Sort By</h4>
        </div>
        
        <div class="sort-options">
            <button class="sort-option ${currentSort.by === 'name' && currentSort.order === 'asc' ? 'active' : ''}" 
                    onclick="applySortAndClose('name', 'asc')">
                <i class="fas fa-sort-alpha-down"></i>
                <span>Name (A-Z)</span>
                ${currentSort.by === 'name' && currentSort.order === 'asc' ? '<i class="fas fa-check"></i>' : ''}
            </button>
            
            <button class="sort-option ${currentSort.by === 'name' && currentSort.order === 'desc' ? 'active' : ''}" 
                    onclick="applySortAndClose('name', 'desc')">
                <i class="fas fa-sort-alpha-up"></i>
                <span>Name (Z-A)</span>
                ${currentSort.by === 'name' && currentSort.order === 'desc' ? '<i class="fas fa-check"></i>' : ''}
            </button>
            
            <button class="sort-option ${currentSort.by === 'city' && currentSort.order === 'asc' ? 'active' : ''}" 
                    onclick="applySortAndClose('city', 'asc')">
                <i class="fas fa-city"></i>
                <span>City (A-Z)</span>
                ${currentSort.by === 'city' && currentSort.order === 'asc' ? '<i class="fas fa-check"></i>' : ''}
            </button>
            
            <button class="sort-option ${currentSort.by === 'city' && currentSort.order === 'desc' ? 'active' : ''}" 
                    onclick="applySortAndClose('city', 'desc')">
                <i class="fas fa-city"></i>
                <span>City (Z-A)</span>
                ${currentSort.by === 'city' && currentSort.order === 'desc' ? '<i class="fas fa-check"></i>' : ''}
            </button>
            
            <button class="sort-option ${currentSort.by === 'sites' && currentSort.order === 'desc' ? 'active' : ''}" 
                    onclick="applySortAndClose('sites', 'desc')">
                <i class="fas fa-sort-numeric-down"></i>
                <span>Policies(High to Low)</span>
                ${currentSort.by === 'sites' && currentSort.order === 'desc' ? '<i class="fas fa-check"></i>' : ''}
            </button>
            
            <button class="sort-option ${currentSort.by === 'sites' && currentSort.order === 'asc' ? 'active' : ''}" 
                    onclick="applySortAndClose('sites', 'asc')">
                <i class="fas fa-sort-numeric-up"></i>
                <span>Policies (Low to High)</span>
                ${currentSort.by === 'sites' && currentSort.order === 'asc' ? '<i class="fas fa-check"></i>' : ''}
            </button>
            
            <button class="sort-option ${currentSort.by === 'date' && currentSort.order === 'desc' ? 'active' : ''}" 
                    onclick="applySortAndClose('date', 'desc')">
                <i class="fas fa-calendar-alt"></i>
                <span>Date (Newest First)</span>
                ${currentSort.by === 'date' && currentSort.order === 'desc' ? '<i class="fas fa-check"></i>' : ''}
            </button>
            
            <button class="sort-option ${currentSort.by === 'date' && currentSort.order === 'asc' ? 'active' : ''}" 
                    onclick="applySortAndClose('date', 'asc')">
                <i class="fas fa-calendar-alt"></i>
                <span>Date (Oldest First)</span>
                ${currentSort.by === 'date' && currentSort.order === 'asc' ? '<i class="fas fa-check"></i>' : ''}
            </button>
        </div>
        
        <div class="dropdown-footer">
            <button class="btn-secondary" onclick="clearSort()">Clear Sort</button>
        </div>
    `;
    
    sortBtn.parentElement.style.position = 'relative';
    sortBtn.parentElement.appendChild(dropdown);
}

function applySortAndClose(sortBy, order) {
    currentSort.by = sortBy;
    currentSort.order = order;
    
    filterAndRenderLocations();
    closeSortDropdown();
    updateSortButtonText();
}

function clearSort() {
    currentSort.by = null;
    currentSort.order = 'asc';
    
    filterAndRenderLocations();
    closeSortDropdown();
    updateSortButtonText();
}

function closeSortDropdown() {
    const dropdown = document.querySelector('.sort-dropdown');
    if (dropdown) {
        dropdown.remove();
    }
}

function updateSortButtonText() {
    const sortBtn = document.getElementById('sortBtn');
    const textSpan = sortBtn.querySelector('.sort-btn-text');
    
    if (!textSpan) {
        const icon = sortBtn.querySelector('i.fa-list');
        const text = document.createTextNode(' Sort By ');
        const chevron = sortBtn.querySelector('i.fa-chevron-down');
        
        const span = document.createElement('span');
        span.className = 'sort-btn-text';
        span.textContent = 'Sort By';
        
        sortBtn.innerHTML = '';
        sortBtn.appendChild(icon);
        sortBtn.appendChild(document.createTextNode(' '));
        sortBtn.appendChild(span);
        sortBtn.appendChild(document.createTextNode(' '));
        sortBtn.appendChild(chevron);
    }
    
    if (currentSort.by) {
        let sortText = '';
        switch(currentSort.by) {
            case 'name':
                sortText = currentSort.order === 'asc' ? 'Name (A-Z)' : 'Name (Z-A)';
                break;
            case 'city':
                sortText = currentSort.order === 'asc' ? 'City (A-Z)' : 'City (Z-A)';
                break;
            case 'sites':
                sortText = currentSort.order === 'desc' ? 'Policies (High-Low)' : 'Policies (Low-High)';
                break;
            case 'date':
                sortText = currentSort.order === 'desc' ? 'Date (Newest)' : 'Date (Oldest)';
                break;
        }
        textSpan.textContent = sortText;
    } else {
        textSpan.textContent = 'Sort By';
    }
}

function filterAndRenderLocations() {
    let filtered = getAllLocations();
    
    if (currentSearchTerm && currentSearchTerm.length > 0) {
        filtered = searchLocations(currentSearchTerm);
    }
    
    if (currentFilters.cofsCompleted !== null || currentFilters.assessmentCompleted !== null) {
        filtered = filtered.filter(location => {
            const cofsMatch = currentFilters.cofsCompleted === null || 
                            location.cofsCompleted === currentFilters.cofsCompleted;
            const assessmentMatch = currentFilters.assessmentCompleted === null || 
                                  location.assessmentCompleted === currentFilters.assessmentCompleted;
            return cofsMatch && assessmentMatch;
        });
    }
    
    if (currentSort.by) {
        filtered = [...filtered].sort((a, b) => {
            let aValue, bValue;
            
            switch(currentSort.by) {
                case 'name':
                    aValue = a.name.toLowerCase();
                    bValue = b.name.toLowerCase();
                    break;
                case 'city':
                    aValue = a.city.toLowerCase();
                    bValue = b.city.toLowerCase();
                    break;
                case 'sites':
                    aValue = a.sitesAffected;
                    bValue = b.sitesAffected;
                    break;
                case 'date':
                    aValue = a.cofsDate ? new Date(a.cofsDate) : new Date(0);
                    bValue = b.cofsDate ? new Date(b.cofsDate) : new Date(0);
                    break;
                default:
                    return 0;
            }
            
            if (aValue < bValue) return currentSort.order === 'asc' ? -1 : 1;
            if (aValue > bValue) return currentSort.order === 'asc' ? 1 : -1;
            return 0;
        });
    }
    
    currentLocations = filtered;
    displayedCount = loadIncrement;
    
    renderLocations();
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeFiltersAndSort);
} else {
    initializeFiltersAndSort();
}