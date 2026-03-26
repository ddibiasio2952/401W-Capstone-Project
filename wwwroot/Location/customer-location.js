/* DAN EDIT 11/27/2025 AM */
/* RENDER MAP */
async function renderMap() {
    try {
        const params = new URLSearchParams(window.location.search);
        let location_id = params.get("location_id");

        // Fallback to localStorage if missing
        if (!location_id) {
            location_id = localStorage.getItem("location_id");
        }

        if (location_id) {
            localStorage.setItem("location_id", location_id);
        } else {
            console.warn("Location id not found!");
            return;
        }

        const loc_response = await fetch(`https://localhost:7288/api/maps/${location_id}`);

        if (!loc_response.ok) {
            throw new Error("Failed to fetch map coordinates!");
        }

        const loc_result = await loc_response.json();


        const latitude = loc_result.latitude;
        const longitude = loc_result.longitude;

        if (address_begin.textContent === "Address") {
            address_begin.textContent = loc_result.address;
        }


        if (isNaN(latitude) || isNaN(longitude)) {
            throw new Error("Lat & lng are not valid numbers!");
        }

        const map_prop = {
            mapId: "b4899a90d947649f1820ad83",
            center: { lat: latitude, lng: longitude },
            zoom: 14,
        };

        map = new google.maps.Map(document.querySelector(".map"), map_prop);

        const anchor = new google.maps.marker.PinElement({
            background: "#2c4275",
            borderColor: "#2c4275",
            glyphSrc: "../image/anchor.jpg",
            scale: 1.3,
        });

        const marker = new google.maps.marker.AdvancedMarkerElement({
            map,
            position: { lat: latitude, lng: longitude },
            content: anchor.element,
        });

        // Get Customer ID from Location JSON
        let customer_id = loc_result.customer_id;

        // If found, store it so it's always available later
        if (customer_id) {
            localStorage.setItem('customer_id', customer_id);
        } else {
            throw new Error('Customer ID not found!');
        }

        // Add Assessment Info

        // Update COPE Assessment trackers
        let cope_status = loc_result.cope_eval;
        let cope_header = document.getElementById('cope_status');
        let cope_date = '';
        if (!loc_result.cope_date) {
            cope_date = 'N/A';
        }
        else {
            cope_date = new Date(loc_result.cope_date + 'T12:00:00').toDateString('en-US');
        }

        if (cope_status == true) { 
            supplement_icon.textContent = '✓';
            supplement_icon.style.color = '#4caf50';
            supplement_date.textContent = cope_date;
            cope_header.innerHTML = ' COPE Supplement Complete';
        } else {
            supplement_icon.textContent = 'X';
            supplement_icon.style.color = '#C90606';
            supplement_date.textContent = cope_date;
            cope_header.innerHTML = ' COPE Supplement Incomplete';
        }

        // Update On-Site Assessment trackers
        let site_status = loc_result.site_eval;
        let site_header = document.getElementById('site_status');
        let site_date = '';
        if (!loc_result.site_date) {
            site_date = 'N/A';
        }
        else {
            site_date = new Date(loc_result.site_date + 'T12:00:00').toDateString('en-US');
        }

        if (site_status == true) {
            completion_icon.textContent = '✓';
            completion_icon.style.color = '#4caf50';
            completion_date.textContent = site_date;
            site_header.innerHTML = 'On-Site Assessment Complete';
        } else {
            completion_icon.textContent = 'X';
            completion_icon.style.color = '#C90606';
            completion_date.textContent = site_date;
            site_header.innerHTML = 'On-Site Assessment Incomplete';
        }

        // Populate Policy Entry Form with location
        const row = document.createElement('option');

        row.value = loc_result.location_id;
        row.textContent = loc_result.address;
        policyLocation.appendChild(row);
    } catch (error) {
        console.log("ERROR", error);
    }
}

/* SET MAP*/
function setMapCoords(lat, lng) {
    const map = document.getElementById("map");
    map.src = `https://www.google.com/maps?q=${lat},${lng}&output=embed`;
}

/* GET EMPLOYEES */
async function getEmployees() {
    try {
        const response = await fetch(`https://localhost:7288/api/employees`);

        // Return error if response fails
        if (!response.ok) {
            const backError = await response.text();
            alert(backError);
            alert('Unable to get employee.');
            throw new Error(backError);
        }

        // Assign JSON object in a variable
        const employees = await response.json();

        // Return error if response is empty
        if (employees.length == 0) {
            console.error(`Employee does not exist in database.`);
        }

        let employeeIdArray = []; // Create employee ID array for input forms
        let employeeNameArray = []; // Create employee name array for input forms

        // Populate policies table with data
        for (const employee of employees) {
            employeeIdArray.push(employee.employee_id); // Add employee ID to array
            employeeNameArray.push(employee.name); // Add employee name to array
        };

        // Add employee Id + name pairs to array
        const employeeArray = [];
        for (let i = 0; i < employeeIdArray.length; i++) {
            const employeeEntry = {
                ID: employeeIdArray[i],
                name: employeeNameArray[i]
            };
            employeeArray.push(employeeEntry);
        }
        
        // Populate Policy Entry Form with employees
        employeeArray.forEach(employee => {
            const row = document.createElement('option');

            row.value = employee.ID;
            row.textContent = employee.name;
            policyAssignedTo.appendChild(row);
        });

        sessionStorage.setItem('employeeArray', JSON.stringify(employeeArray));
    } catch (error) {
        console.error(`Error: ${error}`);
    }
}

/* GET CUSTOMER ID AND FILL TABLES */
async function getCustomer() {
    try {
        const params = new URLSearchParams(window.location.search);
        let customer_id = params.get('customer_id');

        // If not in URL, get it from localStorage
        if (!customer_id) {
            customer_id = localStorage.getItem('customer_id');
        }

        // If found, store it so it's always available later
        if (customer_id) {
            localStorage.setItem('customer_id', customer_id);
        } else {
            throw new Error('Customer ID not found!');
        }

        // Catch error
    } catch (error) {
        console.error(`Error: ${error}`);
    }
}


/* POLICY MODAL FORM */
const policyModal = document.getElementById('modal');
const policyAssignedTo = document.getElementById('manager_id');
const policyLocation = document.getElementById('location_id');

function openPolicyForm() {
    policyModal.classList.add('show-modal');
    autofillPolicyInfo();
}

function closePolicyForm() {
    policyModal.classList.remove('show-modal');
    document.getElementById('policy-input-form').reset();
}

/* AUTOFILL FOR EDIT POLICY */
function autofillPolicyInfo() {
    const autofill = sessionStorage.getItem('policyInput');

    const input = JSON.parse(autofill);
    const policy_inputs = document.getElementById('policy-input-form');

    for (const field of policy_inputs.elements) {
        if (field.name && input.hasOwnProperty(field.name)) {
            if (field.type === 'checkbox') {
                field.checked = !!input[field.name];
            }
            else {
                field.value = input[field.name] ?? '';
            }
        }
    }
}

/* GET POLICIES */
async function getPolicies(location_id) {
    try {
        const response = await fetch(`https://localhost:7288/api/policies/search?location_id=${location_id}`);

        // Return error if response fails
        if (!response.ok) {
            const backError = await response.text();
            alert(backError);
            alert('Unable to get claim.');
            throw new Error(backError);
        }

        // Assign JSON object in a variable
        const policies = await response.json();

        // Return error if variable is empty
        /*if (policies.length == 0) {
            alert('Policy does not exist in database for this location.');
        }*/

        // Update exposure total
        let exposure_total = 0;
        policies.forEach(policy => {
            exposure_total += policy.exposure_amount;
        });
        exposure_box.textContent = `$${exposure_total} USD`;

        // Update Active Policies counter
        let active_policies = 0;
        policies.forEach(policy => {
            if (policy.status == 'Active') {
                active_policies += 1;
            }
        });

        active_policies_circle.textContent = active_policies;

        // Populate Add File Form with policies
        const table = document.querySelector('#file-table tbody');
        table.innerHTML = ''; // Clear existing rows
        filePolicyId.innerHTML = ''; // Clear existing dropdown menu
        for (const policy of policies) {
            await getFiles(policy.policy_id);
            const row = document.createElement('option');

            row.value = policy.policy_id;
            row.textContent = policy.policy_id;
            filePolicyId.appendChild(row); // Populate dropdown menu
        };

        // Send Customer ID and policy address to New Policy form
        document.getElementById('customer_id').value = localStorage.getItem('customer_id');
        document.getElementById('location_id').value = localStorage.getItem('location_id');
        

        // Save form info for later use
        let saved_form = {};

        const policy_inputs = document.getElementById('policy-input-form');
        for (const field of policy_inputs.elements) {
            if (field.name && field.type !== 'submit') {
                if (field.type === 'checkbox') {
                    saved_form[field.name] = field.checked;
                } //
                else {
                    saved_form[field.name] = field.value;
                }
            }
        }
        sessionStorage.setItem('policyInput', JSON.stringify(saved_form));
        autofillPolicyInfo();

    } catch (error) { // Catch error
        console.error(`Error: ${error}`);
    }
}

/* ADD POLICY */
async function addPolicy() {

    const add_created_at = new Date(Date.now() - 5 * 60 * 60 * 1000);

    const policy = {
        account_number: document.getElementById('account_number').value,
        location_id: Number(document.getElementById('location_id').value),
        customer_id: Number(document.getElementById('customer_id').value),
        manager_id: Number(document.getElementById('manager_id').value),
        policy_type: document.getElementById('policy_type').value,
        status: document.getElementById('policy_status').value,
        start_date: document.getElementById('start_date').value,
        end_date: document.getElementById('end_date').value,
        exposure_amount: Number(document.getElementById('exposure_amount').value),
        created_at: add_created_at,
        
    };
    console.log('Submission:', JSON.stringify(policy));  
    fetch('https://localhost:7288/api/policies', {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(policy)
    })
        .then(async response => {  
            if (!response.ok) {
                const backError = await response.text();
                alert(backError);
                throw new Error(backError);
                alert('Unable to add policy. ');
            }
            return response.json();
        })
        .then(form_data => {
            alert('New policy ID: ' + form_data.policy_id);
            closePolicyForm();
            // Dynamically update trackers
            if (form_data.status == 'Active') {
                active_policies_circle.textContent = Number(active_policies_circle.textContent) + 1;
            }
            let current = parseFloat(exposure_box.textContent.replace(/[^0-9.-]/g, '')) || 0;
            let addition = Number(form_data.exposure_amount);
            let updated_exposure = current + addition;
            
            console.log('Addition', addition);
            console.log('Current', current);
            console.log('Updated Exposure', updated_exposure);

            exposure_box.textContent = `$${updated_exposure} USD`;
        })
        .catch(error => {
            console.error('Unable to add policy.', error);
        })
}

/* EVENT LISTENER */
document.addEventListener("DOMContentLoaded", () => {
    /* TOGGLE SUBMIT FILE BUTTON */
    var upload_box = document.getElementById('file-form');
    var submit_file_button = document.getElementById('submit-file-btn');

    upload_box.addEventListener('input', function () {
        if (upload_box.checkValidity()) {
            submit_file_button.disabled = false;
        } else {
            submit_file_button.disabled = true;
        }
    });
});

/* FILE MANAGEMENT */
const fileInput = document.getElementById("fileInput");
const dropZone = document.getElementById("dropZone");
const tableBody = document.querySelector(".files-uploaded tbody");

/* SHOW FILE NAME */
function handleFiles(files) {
    Array.from(files).forEach((file) => {
        document.getElementById('file_name').value = file.name;
    });
}

/* HANDLE DRAG & DROP */
dropZone.addEventListener("dragover", (e) => {
    e.preventDefault();
    dropZone.style.backgroundColor = "#e0e7ff";
});
dropZone.addEventListener("dragleave", () => {
    dropZone.style.backgroundColor = "";
});
dropZone.addEventListener("drop", (e) => {
    e.preventDefault();
    dropZone.style.backgroundColor = "";
    handleFiles(e.dataTransfer.files);
});

/* HANDLE CLICK & BROWSE */
dropZone.addEventListener("click", () => fileInput.click());
fileInput.addEventListener("change", (e) => handleFiles(e.target.files));

/* GET FILES BY POLICY */
async function getFiles(policy_id) {
    try {
        const response = await fetch(`https://localhost:7288/api/UploadedFiles/policysearch?policy_id=${policy_id}`);

        // Return error if response fails
        if (!response.ok) {
            console.error(`Error: ${response.status}`);
        }

        // Assign JSON object in a variable
        const files = await response.json();

        // Return error if variable is empty
        if (files.length == 0) {
            console.error('Policy has no files in database.');
        }

        let fileArray = []; // Create file array 

        // Populate files table with data
        const table = document.querySelector('#file-table tbody');
        files.forEach(file => {
            fileArray.push(file.file_id); // Add file to array
            const date = new Date(file.created_at).toDateString('en-US');
            const row = document.createElement('tr');
            let media_type_ = '';
            // Change MIME type for docx
            if (file.media_type == 'application/vnd.openxmlformats-officedocument.wordprocessingml.document/docx') {
                media_type_ = 'text/docx';
            }
            else {
                media_type_ = file.media_type;
            }
            row.innerHTML = `
            <td>${file.policy_id}</td>
            <td>${file.file_name}</td>
            <td>${media_type_}</td>
            <td>${date}</td>
            <td><button id='download-file-btn' class='download-item-btn' onclick='downloadFile(${file.file_id}, "${file.file_name}")'>⬇</button>
            <button id='delete-file-btn' class='delete-item-btn' onclick='deleteFile(${file.file_id})'>🗑️</button></td>
            `;
            table.appendChild(row);
        });
        console.log('Table populated with', files.length, 'rows');

        // Save file array to session storage
        sessionStorage.setItem("fileArray", JSON.stringify(fileArray));

        // Catch error
    } catch (error) {
        console.error(`Error: ${error}`);
    }
}

/* ADD FILE */
const filePolicyId = document.getElementById('file_policy_id');

async function addFile() {

    const file_created_at = new Date(Date.now() - 5 * 60 * 60 * 1000);
    // Looks in HTML for 'file' and adds to array
    const user_file = document.getElementById('fileInput').files[0];
    // Create FormData() and append file
    const file_data = new FormData();

    file_data.append('fileInput', user_file); // Appends file bytes
    file_data.append('file_name', fileInput.name.value);
    file_data.append('customer_id', Number(localStorage.getItem('customer_id')));
    file_data.append('policy_id', Number(document.getElementById('file_policy_id').value));

    fetch('https://localhost:7288/api/UploadedFiles/upload', {
        method: 'POST',
        body: file_data
    })
        .then(async response => {  
            if (!response.ok) {
                const backError = await response.text();
                alert(backError);
                alert('Unable to add file.');
                throw new Error(backError);
            }
        })
        .then(form_data => {
            alert("File uploaded.");
            document.getElementById('file_policy_id').value = '';
            document.getElementById('file_name').value = '';
            getPolicies(localStorage.getItem('location_id'));
        })
        .catch(error => {
            console.error('Caught Error:', error.message);
            alert('Upload failed.', error.message);
        });
}

/* DOWNLOAD FILE */
async function downloadFile(file_id, file_name) {
    if (!file_id) {
        alert('No file ID chosen.');

        return;
    }

    try {
        const response = await fetch(`https://localhost:7288/api/UploadedFiles/${file_id}`, {
            method: 'GET',
            responseType: 'blob'
        })
            .then((response) => response.blob())
            .then((response) => {
                const url = document.createElement('a');
                url.setAttribute('download', file_name); // Creates download dialog, sets name
                const href = URL.createObjectURL(response);
                url.href = href;

                url.setAttribute('target', '_blank'); // Opens download in a new window
                url.click();
                URL.revokeObjectURL(href);
            });
    } catch (error) {
        console.error(`Error: ${error}`);
    }
}

/* DELETE FILE */
async function deleteFile(file_id) {
    if (!file_id) {
        alert('No file ID chosen.');

        return;
    }

    if (confirm('Are you sure you want to delete the file?')) {
        try {
            const response = await fetch(`https://localhost:7288/api/UploadedFiles/${file_id}`, {
                method: 'DELETE',
                headers: {
                    "Accept": 'application/json',
                },
            })
                .then(() => {
                    alert('File deleted.');
                    getPolicies(localStorage.getItem('location_id'));
                })
        } catch (error) {
            console.error(`Error: ${error}`);
        }
    } else {
        return false;
    }
}

/* COMPANY SELECTOR */
const companySelect = document.querySelector(".company_select");
if (companySelect) {
    companySelect.addEventListener("change", () => {
        const selected = companySelect.value;
        console.log(`Selected company: ${selected}`);

        const notice = document.createElement("div");
        notice.textContent = `✔ Company switched to ${selected}`;
        notice.style.cssText = `
        position: fixed; 
        bottom: 20px; 
        right: 20px; 
        background: #1a2e59; 
        color: white; 
        padding: 10px 20px; 
        border-radius: 8px; 
        font-size: 14px; 
        z-index: 1000;
      `;
        document.body.appendChild(notice);
        setTimeout(() => notice.remove(), 2500);
    });
}

/* SHARE LOCATION */
const shareBtn = document.querySelector(".share-btn");
shareBtn.addEventListener("click", async () => {
    const locationURL = 'https://localhost:7288/Location/customer-location.html?customer_id=' + localStorage.getItem('customer_id') + '&location_id=' + localStorage.getItem('location_id');
    try {
        await navigator.clipboard.writeText(locationURL);
        alert("📍 Location info copied to clipboard!");
    } catch (err) {
        console.error("Clipboard failed", err);
        alert("Could not copy location link.");
    }
});

/* SIDEBAR */
const sidebar = document.getElementById("sidebar-nav");
const toggleButton = document.getElementById("toggle-btn");
const modal = document.getElementById("user-modal");
const profilePicButton = document.getElementById("profile-picture");
const usernameHeader = document.getElementById("username-topbar");

// Checks to see if user had sidebar closed on reload
// If true then keep it closed and remove transitions
if (localStorage.getItem("sidebarState") === "closed") {
    sidebar.classList.add("close", "reload");
}

// Remove the class that prevents transitions after 60 ms
setTimeout(() => {
    sidebar.classList.remove("reload");
}, 60);

function toggleSidebar() {
    sidebar.classList.toggle("close");
    toggleButton.classList.toggle("rotate");

    if (sidebar.classList.contains("close")) {
        localStorage.setItem("sidebarState", "closed");
    }
    else {
        localStorage.setItem("sidebarState", "open");
    }
}

// Compares which version is greater in number
function getLatestRelease(v1, v2) {
    // Gets rid of of v in "v#.#.#"
    // Splits string at '.' and puts each number in an array
    const v1Nums = v1.replace(/^v/, "").split(".").map(Number);
    const v2Nums = v2.replace(/^v/, "").split(".").map(Number);

    // Compares each number and immediately returns version with higher num
    for (let i = 0; i < 3; i++) {
        if (v1Nums[i] > v2Nums[i]) return v1;
        if (v2Nums[i] > v1Nums[i]) return v2;
    }

    // If equal, just returns one of them
    return v2;
}

async function releaseToSidebar() {
    try {
        const releaseLink = document.getElementById("release-notes");

        // Fetches list of releases
        const response = await fetch('https://localhost:7288/api/releases');

        if(!response.ok) {
            const errorMsg = await response.text();
            throw new Error(errorMsg);
        }
    
        const data = await response.json();

        // Iterates through each release object
        // returns a new array w/ only version numbers
        const releases = !Array.isArray(data) ? [] : data.map(r => r.version)
        if (releases.length === 0) {
            console.warn("No release versions were found");
            return;
        }

        // Executes callback function on each element
        // Compares version numbers to get recent version
        const recentRelease = releases.reduce(getLatestRelease);

        releaseLink.textContent = recentRelease;

    } catch(error) {
        console.error("ERROR:", error);
    }
}

async function usernameToHeader() {
    try {
        const response = await fetch(`https://localhost:7288/api/account/account-name`);

        if(!response.ok) {
            throw new Error("Could not fetch username");
        }

        const username = await response.text();

        usernameHeader.textContent = username;
    }
    catch(errorMsg) {
        console.error(errorMsg);
    }
}

async function signOut() {
    await fetch(`https://localhost:7288/api/auth/logoff`, {
        method: "POST"
    });

    sessionStorage.clear();

    window.location.href = "https://localhost:7288/";

}

profilePicButton.onclick = () => {
    modal.classList.toggle('active');
}

modal.onmouseleave = () => {
    modal.classList.remove("active");
}

document.addEventListener("DOMContentLoaded", async () => {
    await releaseToSidebar();
    await usernameToHeader();
});

/* ONLOAD */
window.onload = async () => {
    await renderMap();
    if (!location_id) return;

    // Load rest of data.
    await getPolicies(localStorage.getItem('location_id'));
    await getCustomer();
    await getEmployees();
};
