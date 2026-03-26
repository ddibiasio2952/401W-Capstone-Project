// Release notes page - connects to the API to show/manage releases

const API_BASE = 'https://localhost:7288/api/releases';
let isAdmin = true; // hardcoded for demonstration
let releases = [];
let quillNotesEditor = null;
let quillHotfixesEditor = null;

// Set up admin stuff if user is admin
if (isAdmin) {
  setupAdminControls();
}

// Load releases when page loads
fetch(API_BASE)
  .then(response => {
    if (!response.ok) throw new Error('Failed to fetch releases');
    return response.json();
  })
  .then(data => {
    releases = data.data || data;
    console.log('Releases loaded:', releases.length);
    renderVersionList();
  })
  .catch(error => {
    console.error('Error fetching releases:', error);
    renderVersionList(); // still show empty list
  });

// ===== Setup Functions =====

function setupAdminControls() {
  document.getElementById('adminControls').style.display = 'block';
  document.getElementById('adminControls').setAttribute('aria-hidden', 'false');
  
  // Add all the event listeners for admin buttons
  document.getElementById('addReleaseBtn').addEventListener('click', () => openReleaseModal('add'));
  document.getElementById('modalClose').addEventListener('click', closeReleaseModal);
  document.getElementById('modalCancel').addEventListener('click', closeReleaseModal);
  document.getElementById('modalBackdrop').addEventListener('click', closeReleaseModal);
  document.getElementById('releaseForm').addEventListener('submit', handleReleaseSubmit);
  document.getElementById('deleteModalClose').addEventListener('click', closeDeleteModal);
  document.getElementById('cancelDeleteBtn').addEventListener('click', closeDeleteModal);
  document.getElementById('confirmDeleteBtn').addEventListener('click', confirmDelete);
  document.getElementById('deleteBackdrop').addEventListener('click', closeDeleteModal);
}

// ===== Helper Functions =====
function parseLocalDate(dateString) {
  if (!dateString) return null;
  const [year, month, day] = dateString.split('-').map(Number);
  return new Date(year, month - 1, day);
}

function formatDate(dateString) {
  if (!dateString) return '—';
  const date = parseLocalDate(dateString);
  return date.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
}

function formatDateShort(dateString) {
  if (!dateString) return '';
  const date = parseLocalDate(dateString);
  return date.toLocaleDateString('en-US');
}

// Get which date to show (complete > rollout > start)
function getDisplayDate(release) {
  return release.complete_date || release.rollout_date || release.start_date || '';
}

// Strip HTML tags to get plain text
function extractPlainText(content) {
  if (!content) return '';
  const temp = document.createElement('div');
  temp.innerHTML = content;
  return (temp.textContent || temp.innerText || '').trim();
}

// Parse version string like "v1.2.3" into parts: [major, minor, patch]
function parseVersion(versionString) {
  const match = versionString.match(/^v?(\d+)\.(\d+)\.(\d+)$/);
  if (!match) return null;
  return {
    major: parseInt(match[1]),
    minor: parseInt(match[2]),
    patch: parseInt(match[3])
  };
}

// Compare two versions, returns 1 if v1 > v2, -1 if v1 < v2, 0 if equal
function compareVersions(v1, v2) {
  const ver1 = parseVersion(v1);
  const ver2 = parseVersion(v2);
  
  if (!ver1 || !ver2) return 0;
  
  if (ver1.major !== ver2.major) return ver1.major - ver2.major;
  if (ver1.minor !== ver2.minor) return ver1.minor - ver2.minor;
  return ver1.patch - ver2.patch;
}

// Make sure dates are in order: start <= rollout <= complete
function validateDateOrder(start_date, rollout_date, complete_date) {
  if (!start_date) return { valid: true };
  
  const start = new Date(start_date);
  
  if (rollout_date && new Date(rollout_date) < start) {
    return { valid: false, message: 'Rollout date must be on or after the start date.' };
  }
  
  if (complete_date) {
    const complete = new Date(complete_date);
    const compareDate = rollout_date ? new Date(rollout_date) : start;
    
    if (complete < compareDate) {
      return {
        valid: false,
        message: rollout_date 
          ? 'Complete date must be on or after the rollout date.'
          : 'Complete date must be on or after the start date.'
      };
    }
  }
  
  return { valid: true };
}

// Check if version format is valid and makes sense with dates
function validateVersionLogic(newVersion, newDate, mode, originalVersion) {
  const parsedVersion = parseVersion(newVersion);
  if (!parsedVersion) {
    return {
      valid: false,
      message: 'Invalid version format. Version must be in the format "vX.X.X" (e.g., v1.0.0).'
    };
  }
  
  for (const release of releases) {
    if (mode === 'edit' && release.version === originalVersion) continue;
    
    if (!release.start_date) continue;
    
    const comparison = compareVersions(newVersion, release.version);
    
    // Can't have duplicate versions
    if (comparison === 0) {
      return {
        valid: false,
        message: `Version ${newVersion} already exists. Please use a different version number.`
      };
    }
    
    const newDateTime = new Date(newDate).getTime();
    const existingDateTime = new Date(release.start_date).getTime();
    
    // Higher versions should have later dates
    if (comparison > 0 && newDateTime < existingDateTime) {
      return {
        valid: false,
        message: `Version ${newVersion} is higher than ${release.version} but has an earlier release date.`
      };
    }
    
    // Lower versions should have earlier dates
    if (comparison < 0 && newDateTime > existingDateTime) {
      return {
        valid: false,
        message: `Version ${newVersion} is lower than ${release.version} but has a later release date.`
      };
    }
  }
  
  return { valid: true };
}

// Handle notes that might be HTML or plain text
function parseNotes(notes) {
  if (!notes) return '<p>No notes available.</p>';
  
  // If it looks like HTML, return it
  if (notes.trim().startsWith('<')) {
    return notes;
  }
  
  // Otherwise escape and wrap in paragraphs
  const escaped = notes
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;');
  return `<p>${escaped.split('\n').join('</p><p>')}</p>`;
}

// Set up the Quill editors
function initializeQuillEditors() {
  if (typeof Quill === 'undefined') {
    console.error('Quill editor library not loaded');
    return;
  }
  
  const toolbarConfig = [
    [{ 'header': [1, 2, 3, false] }],
    ['bold', 'italic', 'underline'],
    [{ 'list': 'ordered'}, { 'list': 'bullet' }],
    ['link'],
    ['clean']
  ];
  
  if (!quillNotesEditor) {
    quillNotesEditor = new Quill('#releaseNotesEditor', {
      theme: 'snow',
      modules: { toolbar: toolbarConfig },
      placeholder: 'Enter features and improvements...'
    });
  }
  
  if (!quillHotfixesEditor) {
    quillHotfixesEditor = new Quill('#releaseHotfixesEditor', {
      theme: 'snow',
      modules: { toolbar: toolbarConfig },
      placeholder: 'Enter bug fixes and patches...'
    });
  }
}

function showError(errorId, message) {
  const el = document.getElementById(errorId);
  el.textContent = message;
  el.style.display = 'block';
}

function clearError(errorId) {
  const el = document.getElementById(errorId);
  el.textContent = '';
  el.style.display = 'none';
}

async function refreshReleases() {
  const response = await fetch(API_BASE);
  if (!response.ok) throw new Error('Failed to refresh releases');
  const data = await response.json();
  releases = data.data || data;
}

// ===== Rendering =====

// Show the details for a specific release
function renderDetails(release) {
  document.getElementById('detailsTitle').textContent = release.summary;
  document.getElementById('detailsVersion').textContent = release.version;
  document.getElementById('detailsStartDate').textContent = formatDate(release.start_date);
  document.getElementById('detailsRolloutDate').textContent = formatDate(release.rollout_date);
  document.getElementById('detailsCompleteDate').textContent = formatDate(release.complete_date);
  
  // Features section
  const featuresEl = document.getElementById('detailsFeatures');
  featuresEl.style.display = '';
  featuresEl.innerHTML = '<h3>Features</h3>' + 
    (release.notes ? parseNotes(release.notes) : '<p>No features listed.</p>');

  // Hotfixes section
  const hotfixesEl = document.getElementById('detailsHotfixes');
  if (release.hotfix_notes) {
    hotfixesEl.innerHTML = '<h3>Hotfixes</h3>' + parseNotes(release.hotfix_notes);
    hotfixesEl.style.display = '';
  } else {
    hotfixesEl.style.display = 'none';
  }

  // Show edit and add buttons if admin
  const saveRow = document.querySelector('.details .save-row');
  const addBtn = document.getElementById('addReleaseBtn');
  if (!isAdmin) {
    saveRow.style.display = 'none';
    addBtn.style.display = 'none';
    return;
  }
  
  saveRow.innerHTML = '';
  const editBtn = document.createElement('button');
  editBtn.type = 'button';
  editBtn.className = 'btn-primary';
  editBtn.textContent = 'Edit Release';
  editBtn.addEventListener('click', () => openReleaseModal('edit', release.version));
  
  saveRow.appendChild(editBtn);
  saveRow.style.display = 'flex';
  saveRow.style.gap = '0.5rem';
}

// Render the list of versions on the left
function renderVersionList() {
  const list = document.getElementById('versionList');
  const select = document.getElementById('versionSelect');
  list.innerHTML = '';
  select.innerHTML = '';

  if (releases.length === 0) {
    list.innerHTML = '<li style="padding: 1rem; color: #6b7280;">No releases yet. Click "Add Release" to create one.</li>';
    return;
  }

  // Sort by version (newest first)
  releases.sort((a, b) => {
    const comparison = compareVersions(b.version, a.version);
    if (comparison !== 0) return comparison;
    
    // If versions are equal, sort by date
    const dateA = getDisplayDate(a) ? new Date(getDisplayDate(a)) : new Date(0);
    const dateB = getDisplayDate(b) ? new Date(getDisplayDate(b)) : new Date(0);
    return dateB - dateA;
  });

  releases.forEach(release => {
    list.appendChild(createVersionCard(release));
    select.appendChild(createVersionOption(release));
  });

  // Mobile dropdown handler
  select.onchange = () => {
    const release = releases.find(r => r.version === select.value);
    if (release) renderDetails(release);
  };

  // Show first release by default
  if (releases.length > 0) {
    renderDetails(releases[0]);
  }
}

// Create a card for the version list
function createVersionCard(release) {
  const summary = release.summary ? release.summary.trim() : '';
  const releaseDate = formatDateShort(getDisplayDate(release));

  const li = document.createElement('li');
  li.className = 'card-item version-item';
  li.setAttribute('data-version', release.version);
  
  const body = document.createElement('div');
  body.className = 'card-body';
  
  // Top row: version + summary, details button
  const topRow = document.createElement('div');
  topRow.className = 'card-top-row';
  
  const titleRow = document.createElement('div');
  titleRow.className = 'card-title-row';
  
  const title = document.createElement('div');
  title.className = 'card-title';
  title.textContent = release.version;
  titleRow.appendChild(title);
  
  if (summary) {
    const separator = document.createElement('span');
    separator.className = 'card-title-separator';
    separator.textContent = ' - ';
    titleRow.appendChild(separator);
    
    const summarySpan = document.createElement('span');
    summarySpan.className = 'card-title-summary';
    summarySpan.textContent = summary;
    titleRow.appendChild(summarySpan);
  }
  
  topRow.appendChild(titleRow);
  
  // Details button
  const detailsBtn = document.createElement('button');
  detailsBtn.className = 'details-btn';
  detailsBtn.setAttribute('aria-controls', 'details');
  detailsBtn.setAttribute('aria-expanded', 'false');
  detailsBtn.textContent = 'Details';
  detailsBtn.addEventListener('click', function() {
    document.querySelectorAll('.details-btn').forEach(b => b.setAttribute('aria-expanded', 'false'));
    this.setAttribute('aria-expanded', 'true');
    renderDetails(release);
  });
  topRow.appendChild(detailsBtn);
  body.appendChild(topRow);
  
  // Bottom row: date, edit/delete buttons
  const bottomRow = document.createElement('div');
  bottomRow.className = 'card-bottom-row';
  
  if (releaseDate) {
    const meta = document.createElement('div');
    meta.className = 'card-meta';
    meta.textContent = releaseDate;
    bottomRow.appendChild(meta);
  }
  
  if (isAdmin) {
    const actions = document.createElement('div');
    actions.className = 'card-actions';
    
    const editBtn = document.createElement('button');
    editBtn.className = 'action-btn edit';
    editBtn.innerHTML = '<span aria-hidden="true">✎</span><span class="label">Edit</span>';
    editBtn.addEventListener('click', (e) => {
      e.stopPropagation();
      openReleaseModal('edit', release.version);
    });
    
    const deleteBtn = document.createElement('button');
    deleteBtn.className = 'action-btn delete';
    deleteBtn.innerHTML = '<span aria-hidden="true">🗑</span><span class="label">Delete</span>';
    deleteBtn.addEventListener('click', (e) => {
      e.stopPropagation();
      openDeleteModal(release.version);
    });
    
    actions.appendChild(editBtn);
    actions.appendChild(deleteBtn);
    bottomRow.appendChild(actions);
  }
  
  body.appendChild(bottomRow);
  li.appendChild(body);
  
  return li;
}

// Create option for dropdown
function createVersionOption(release) {
  const summary = release.summary ? release.summary.trim() : '';
  const opt = document.createElement('option');
  opt.value = release.version;
  
  const fullText = `${release.version}${summary ? ` - ${summary}` : ''}`;
  opt.textContent = fullText;
  opt.title = fullText; // full text on hover
  
  return opt;
}

// ===== Modals =====

function openReleaseModal(mode, version = null) {
  clearError('modalError');
  const modal = document.getElementById('releaseModal');
  const form = document.getElementById('releaseForm');
  const versionInput = document.getElementById('releaseVersion');
  
  initializeQuillEditors();
  
  document.getElementById('modalTitle').textContent = mode === 'add' ? 'Add Release' : 'Edit Release';
  document.getElementById('formMode').value = mode;

  if (mode === 'edit') {
    const release = releases.find(r => r.version === version);
    if (!release) {
      showError('modalError', 'Release not found');
      return;
    }
    
    versionInput.value = release.version;
    versionInput.disabled = true;
    document.getElementById('releaseSummary').value = release.summary || '';
    document.getElementById('releaseStartDate').value = release.start_date || '';
    document.getElementById('releaseRolloutDate').value = release.rollout_date || '';
    document.getElementById('releaseCompleteDate').value = release.complete_date || '';
    
    // Load content into editors using Quill's API
    if (quillNotesEditor && quillHotfixesEditor) {
      if (release.notes) {
        quillNotesEditor.clipboard.dangerouslyPasteHTML(release.notes);
      } else {
        quillNotesEditor.setContents([]);
      }
      
      if (release.hotfix_notes) {
        quillHotfixesEditor.clipboard.dangerouslyPasteHTML(release.hotfix_notes);
      } else {
        quillHotfixesEditor.setContents([]);
      }
    }
    
    document.getElementById('originalVersion').value = release.version;
  } else {
    form.reset();
    versionInput.disabled = false;
    document.getElementById('releaseStartDate').value = new Date().toISOString().slice(0, 10);
    document.getElementById('originalVersion').value = '';
    
    // Clear editors
    if (quillNotesEditor && quillHotfixesEditor) {
      quillNotesEditor.setContents([]);
      quillHotfixesEditor.setContents([]);
    }
  }

  modal.style.display = 'block';
  modal.setAttribute('aria-hidden', 'false');
  versionInput.focus();
  
  document.addEventListener('keydown', escapeHandler);
}

function closeReleaseModal() {
  const modal = document.getElementById('releaseModal');
  modal.style.display = 'none';
  modal.setAttribute('aria-hidden', 'true');
  document.removeEventListener('keydown', escapeHandler);
}

async function handleReleaseSubmit(e) {
  e.preventDefault();
  
  const mode = document.getElementById('formMode').value;
  const originalVersion = document.getElementById('originalVersion').value;
  const version = document.getElementById('releaseVersion').value.trim();
  const summary = document.getElementById('releaseSummary').value.trim();
  const start_date = document.getElementById('releaseStartDate').value;
  const rollout_date = document.getElementById('releaseRolloutDate').value || null;
  const complete_date = document.getElementById('releaseCompleteDate').value || null;
  
  if (!quillNotesEditor || !quillHotfixesEditor) {
    showError('modalError', 'Editors not initialized. Please refresh the page.');
    return;
  }
  
  const notesHtml = quillNotesEditor.root.innerHTML.trim();
  const hotfixesHtml = quillHotfixesEditor.root.innerHTML.trim();
  // Quill gives '<p><br></p>' for empty content
  const notes = (notesHtml && notesHtml !== '<p><br></p>') ? notesHtml : null;
  const hotfix_notes = (hotfixesHtml && hotfixesHtml !== '<p><br></p>') ? hotfixesHtml : null;

  // Character limits (from database)
  const LIMITS = {
    version: 10,
    summary: 50,
    notes: 400,
    hotfix_notes: 400
  };

  // Validation
  if (!version) return showError('modalError', 'Version is required');
  if (!summary) return showError('modalError', 'Summary/Title is required');
  if (!start_date) return showError('modalError', 'Start date is required');
  
  if (version.length > LIMITS.version) {
    return showError('modalError', `Version cannot exceed ${LIMITS.version} characters. Current length: ${version.length}`);
  }
  
  if (summary.length > LIMITS.summary) {
    return showError('modalError', `Summary/Title cannot exceed ${LIMITS.summary} characters. Current length: ${summary.length}`);
  }
  
  // Validate HTML content length (what actually gets stored in database)
  if (notes && notesHtml.length > LIMITS.notes) {
    const notesPlainText = extractPlainText(notes);
    return showError('modalError', `Features content is too long. HTML content (${notesHtml.length} chars) exceeds the ${LIMITS.notes} character limit. Plain text length: ${notesPlainText.length} characters.`);
  }
  
  if (hotfix_notes && hotfixesHtml.length > LIMITS.hotfix_notes) {
    const hotfixesPlainText = extractPlainText(hotfix_notes);
    return showError('modalError', `Hotfixes content is too long. HTML content (${hotfixesHtml.length} chars) exceeds the ${LIMITS.hotfix_notes} character limit. Plain text length: ${hotfixesPlainText.length} characters.`);
  }
  
  // Check date year is reasonable
  const year = new Date(start_date).getFullYear();
  const currentYear = new Date().getFullYear();
  if (year < currentYear - 1 || year > currentYear + 5) {
    return showError('modalError', `Start date year must be between ${currentYear - 1} and ${currentYear + 5}`);
  }
  
  // Validate dates and version logic
  const dateOrderCheck = validateDateOrder(start_date, rollout_date, complete_date);
  if (!dateOrderCheck.valid) {
    return showError('modalError', dateOrderCheck.message);
  }
  
  const versionCheck = validateVersionLogic(version, start_date, mode, originalVersion);
  if (!versionCheck.valid) {
    return showError('modalError', versionCheck.message);
  }

  const data = {
    version,
    summary,
    start_date,
    rollout_date,
    complete_date,
    notes,
    hotfix_notes
  };

  try {
    const url = mode === 'add' ? API_BASE : `${API_BASE}/${originalVersion}`;
    const response = await fetch(url, {
      method: mode === 'add' ? 'POST' : 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data)
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(JSON.parse(error).error || error);
    }

    await refreshReleases();
    closeReleaseModal();
    renderVersionList();
  } catch (error) {
    console.error('Error saving release:', error);
    showError('modalError', error.message || 'Failed to save release');
  }
}

// Delete modal
let pendingDeleteVersion = null;

function openDeleteModal(version) {
  pendingDeleteVersion = version;
  document.getElementById('deleteVersionLabel').textContent = version;
  
  const modal = document.getElementById('deleteModal');
  modal.style.display = 'block';
  modal.setAttribute('aria-hidden', 'false');
  document.addEventListener('keydown', escapeHandler);
}

function closeDeleteModal() {
  const modal = document.getElementById('deleteModal');
  modal.style.display = 'none';
  modal.setAttribute('aria-hidden', 'true');
  pendingDeleteVersion = null;
  document.removeEventListener('keydown', escapeHandler);
}

async function confirmDelete() {
  if (!pendingDeleteVersion) return closeDeleteModal();
  
  try {
    const response = await fetch(`${API_BASE}/${pendingDeleteVersion}`, {
      method: 'DELETE'
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Failed to delete release');
    }

    await refreshReleases();
    closeDeleteModal();
    renderVersionList();
  } catch (error) {
    console.error('Error deleting release:', error);
    alert('Failed to delete release: ' + error.message);
    closeDeleteModal();
  }
}

// Close modals with Escape key
function escapeHandler(e) {
  if (e.key === 'Escape') {
    const releaseModal = document.getElementById('releaseModal');
    const deleteModal = document.getElementById('deleteModal');
    if (releaseModal.getAttribute('aria-hidden') === 'false') closeReleaseModal();
    if (deleteModal.getAttribute('aria-hidden') === 'false') closeDeleteModal();
  }
}