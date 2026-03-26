/* DAN EDIT 11/29/2025 PM */
/* GET LOCATION */ 
async function getLocation() {
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

        if (address.textContent === "City, State, Zip Code") {
            address.textContent = loc_result.address;
        }

        // Populate Policy Entry Form with location
        const row = document.createElement('option');

        row.value = loc_result.location_id;
        row.textContent = loc_result.address;
        policyLocation.appendChild(row);

        return location_id;

    } catch (error) {
        console.log("ERROR", error);
    }
}

/* CLAIMS VIEW MODAL FORM */
const claimsTableModal = document.getElementById("claimsModal");
const closeClaimsModal = document.getElementById("closeClaimsModal");
const claimsTableBody = document.getElementById("claimsTableBody");
const claimsTotalEl = document.getElementById("claimsTotal");
const claimsOpenEl = document.getElementById("claimsOpen");
const claimsPolicyIdEl = document.getElementById("claimsPolicyId");

closeClaimsModal.addEventListener("click", () =>
    claimsTableModal.classList.remove("active")
);
claimsTableModal.addEventListener("click", (e) => {
    if (e.target === claimsTableModal) claimsTableModal.classList.remove("active");
});

/* CLAIM ENTRY MODAL FORM */
const claimAddModal = document.getElementById('claim-modal');
const claimId = document.createElement('div');
const claimCreate = document.createElement('div');
const claimInputForm = document.querySelector('#claim-input-form');
const claimAssignedTo = document.getElementById('assigned_to');
const claimCreateButton = document.querySelector('#open-claim')
const claimEditButton = document.querySelector('#submit_claim_btn');

async function openClaimForm(policy_id) {
    const user_id = await getUserId();
    
    document.getElementById('claim_policy_id').value = policy_id;
    document.getElementById('created_by').value = user_id;
    
    // Show form and hide table
    claimsTableModal.classList.remove("active");
    claimAddModal.classList.add('show-modal');
}

function closeClaimForm() {
    claimAddModal.classList.remove('show-modal');
    claimsTableModal.classList.add("active")
    document.getElementById('claim-input-form').reset();

    // Remove edit-only properties
    document.getElementById('claim_number').disabled = false;
    claimInputForm.setAttribute('method', 'POST');
    claimInputForm.setAttribute('onsubmit', 'addClaim()');
    claimEditButton.setAttribute('value', 'Add Claim');
    claimId.innerHTML = '';
    claimCreate.innerHTML = '';
    document.getElementById('claim-title').textContent = 'Add Claim';
}

/* GET CLAIMS COUNTER FOR MODAL POPUP */
async function getClaimsCounter(policy_id) {
    try {
        const response = await fetch(`https://localhost:7288/api/claims/search?policy_id=${policy_id}`);

        // Return error if response fails
        if (!response.ok) {
            const backError = await response.text();
            alert(backError);
            alert('Unable to get claim.');
            throw new Error(backError);
        }

        // Assign JSON object in a variable
        const claims = await response.json();

        // Get Total / Open Claims ratio
        const total = claims.length;
        const open = claims.filter(
            (claim) => claim.status.toLowerCase() !== "closed"
        ).length;

        const claims_ratio = `${total} / ${open}`;
        //console.log('Data Returned:', claims_ratio);
        return claims_ratio;
    } catch (error) { // Catch error
        console.error(`Error: ${error}`);
    }
}

/* GET CLAIMS */
async function getClaims(policy_id) {
    try {
        const response = await fetch(`https://localhost:7288/api/claims/search?policy_id=${policy_id}`);

        // Return error if response fails
        if (!response.ok) {
            const backError = await response.text();
            alert(backError);
            alert('Unable to get claim.');
            throw new Error(backError);
        }

        // Assign JSON object in a variable
        const claims = await response.json();

        // Show modal
        claimsTableModal.classList.add("active");

        // Return error if response is empty
        if (claims.length == 0) {
            console.error(`Claim does not exist in database for Policy ID ${policy_id}.`);
            claimsTableBody.innerHTML = ''; // Clear existing rows
            const row = document.createElement('tr');
            row.innerHTML = `
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td></td>
            <td><button class="add-claim-btn" id="open-claim" onclick="openClaimForm(${policy_id})">Add New Claim</button></td>
            `;
            claimsTableBody.appendChild(row);

            // Send Policy ID to "Add New Claim" button
            claimCreateButton.setAttribute('onclick', `openClaimForm(${policy_id})`);

            // Populate top of modal
            claimsTotalEl.textContent = 0;
            claimsOpenEl.textContent = 0;

            claimsPolicyIdEl.textContent = policy_id;
        }
        else {
            // Populate claims table with data
            claimsTableBody.innerHTML = ''; // Clear existing rows
            for (const claim of claims) {
                const row = document.createElement('tr');
                const loss_date = new Date(claim.date_of_loss).toDateString('en-US');
                const report_date = new Date(claim.date_reported).toDateString('en-US');
                const created_date = new Date(claim.date_created).toDateString('en-US');
                console.log("Claim ID:", claim.claim_id);
                row.innerHTML = `
            <td>${claim.claim_number}</td>
            <td>${claim.status}</td>
            <td>${loss_date}</td>
            <td>${report_date}</td>
            <td>$${claim.reserve_amount} USD</td>
            <td>$${claim.paid_amount} USD</td>
            <td>${claim.memo}</td>
            <td>${claim.assigned_employee}</td>
            <td><button id="edit-claim-btn" class="edit-item-btn" onclick='openClaimEditForm(${claim.claim_id}, ${claim.policy_id}, ${JSON.stringify(claim.claim_number)}, 
                ${JSON.stringify(claim.status)}, ${JSON.stringify(claim.date_of_loss)}, ${JSON.stringify(claim.date_reported)}, ${claim.reserve_amount}, 
                ${claim.paid_amount}, ${JSON.stringify(claim.memo)}, ${claim.assigned_to}, ${JSON.stringify(claim.created_at)}
            )'>Edit</button>
            <button id="delete-claim-btn" class="delete-item-btn" onclick="deleteClaim(${claim.claim_id}, ${claim.policy_id})">Delete</button></td>
            `;
                claimsTableBody.appendChild(row);

                // Send Policy ID to "Add New Claim" button
                claimCreateButton.setAttribute('onclick', `openClaimForm(${claim.policy_id})`);

                // Get Total / Open Claims ratio
                let total = claims.length;
                let open = claims.filter(
                    (claim) => claim.status.toLowerCase() !== "closed"
                ).length;

                // Populate top of modal
                claimsTotalEl.textContent = total;
                claimsOpenEl.textContent = open;
                claimsPolicyIdEl.textContent = policy_id;
            };
        }
    } catch (error) { // Catch error
        console.error(`Error: ${error}`);
    }
}

/* ADD NEW CLAIM */
async function addClaim() {

    const add_created_at = new Date(Date.now() - 5 * 60 * 60 * 1000);
    const user_id = await getUserId();

    const claim = {
        policy_id: Number(document.getElementById('claim_policy_id').value),
        claim_number: document.getElementById('claim_number').value,
        status: document.getElementById('claim_status').value,
        date_of_loss: document.getElementById('date_of_loss').value,
        date_reported: document.getElementById('date_reported').value,
        reserve_amount: Number(document.getElementById('reserve_amount').value),
        paid_amount: Number(document.getElementById('paid_amount').value),
        memo: document.getElementById('memo').value,
        assigned_to: Number(document.getElementById('assigned_to').value),
        created_by: user_id,
        created_at: add_created_at
    };
    console.log('Submission:', JSON.stringify(claim));  
    fetch('https://localhost:7288/api/claims', {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(claim)
    })
        .then(async response => {  
            if (!response.ok) {
                const backError = await response.text();
                alert(backError);
                alert('Unable to add claim. ');
                throw new Error(backError);
            }
            return response.json();
        })
        .then(form_data => {
            alert('New claim ID: ' + form_data.claim_id);
            closeClaimForm();
            getClaims(form_data.policy_id);
            getPolicies(localStorage.getItem('location_id'));
        })
        .catch(error => {
            console.error('Unable to add claim.', error);
        })
}

/* EDIT CLAIM */
async function editClaim() {

    const claim_id = document.getElementById('claim_id').value;

    const claim = {
        created_at: document.getElementById('claim_created_at').value,
        policy_id: Number(document.getElementById('claim_policy_id').value),
        claim_number: document.getElementById('claim_number').value,
        status: document.getElementById('claim_status').value,
        date_of_loss: document.getElementById('date_of_loss').value,
        date_reported: document.getElementById('date_reported').value,
        reserve_amount: Number(document.getElementById('reserve_amount').value),
        paid_amount: Number(document.getElementById('paid_amount').value),
        memo: document.getElementById('memo').value,
        assigned_to: Number(document.getElementById('assigned_to').value),
        created_by: Number(document.getElementById('created_by').value)
    };

    console.log('Submission:', JSON.stringify(claim));  
    console.log('Main Body Claim ID:', claim_id);
    fetch(`https://localhost:7288/api/claims/${claim_id}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(claim)
    })
        .then(async response => {  
            if (!response.ok) {
                const backError = await response.text();
                alert(backError);
                alert('Unable to edit claim. ');
                throw new Error(backError);
            }
            return response.json();
        })
        .then(form_data => {
            alert('Edited claim ID: ' + form_data.claim_id);
            // Set attributes to POST
            closeClaimForm();
            getClaims(form_data.policy_id);
            getPolicies(localStorage.getItem('location_id'));
        })
        .catch(error => {
            console.error('Unable to edit claim.', error);
        })
}

/* OPEN CLAIM EDIT FORM */
async function openClaimEditForm(claim_id, policy_id, claim_number,
    status, date_of_loss, date_reported, reserve_amount, paid_amount,
    memo, assigned_to, created_at) {

    // Close Claim Table, open Add Modal
    claimsTableModal.classList.remove("active");
    claimAddModal.classList.add('show-modal');

    const user_id = await getUserId();

    // Convert dates to format which type="date" can read
    let loss_datef = new Date(date_of_loss).toISOString().slice(0, 10);
    let report_datef = new Date(date_reported).toISOString().slice(0, 10);
    let create_datef = new Date(created_at).toISOString().slice(0, 10);

    document.getElementById('claim_number').disabled = true;

    // Fill input fields
    document.getElementById('claim_id').value = claim_id;
    document.getElementById('claim_created_at').value = create_datef;
    document.getElementById('claim_policy_id').value = policy_id;
    document.getElementById('claim_number').value = claim_number;
    document.getElementById('claim_status').value = status;
    document.getElementById('date_of_loss').value = loss_datef;
    document.getElementById('date_reported').value = report_datef;
    document.getElementById('reserve_amount').value = reserve_amount;
    document.getElementById('paid_amount').value = paid_amount;
    document.getElementById('memo').value = memo;
    document.getElementById('assigned_to').value = assigned_to;
    document.getElementById('created_by').value = user_id;

    // Set attributes to PUT
    claimInputForm.setAttribute('method', 'PUT');
    claimInputForm.setAttribute('onsubmit', 'editClaim()');
    claimEditButton.setAttribute('value', 'Edit Claim');
    document.getElementById('claim-title').textContent = 'Edit Claim';
}

/* DELETE CLAIM */
async function deleteClaim(claim_id, policy_id) {
    if (!claim_id) {
        alert('No claim ID found.');
        return;
    }
    // Confirm
    if (confirm('Are you sure you want to delete the claim?')) {
        try {
            const response = await fetch(`https://localhost:7288/api/claims/${claim_id}`, {
                method: 'DELETE',
                headers: {
                    'Accept': 'application/json',
                },
            })
                .then(() => {
                    alert('Claim deleted.');
                    getClaims(policy_id);
                    getPolicies(localStorage.getItem('location_id'));
                })
        } catch (error) {
            console.error(`Error: ${error}`);
        }
    } else {
        return false;
    }
}

/* POLICY MODAL FORM */
const policyModal = document.getElementById('policy-modal');
const polId = document.createElement('div');
const polCreate = document.createElement('div');
const policyInputForm = document.querySelector('#policy-input-form');
const policyEditButton = document.querySelector('#submit_policy_btn');
const policyAssignedTo = document.getElementById('manager_id');
const policyLocation = document.getElementById('location_id');

function openPolicyForm() {
    policyModal.classList.add('show-modal');
    autofillPolicyInfo();
}

function closePolicyForm() {
    policyModal.classList.remove('show-modal');
    document.getElementById('policy-input-form').reset();

    // Remove edit-only properties
    policyInputForm.setAttribute('method', 'POST');
    policyInputForm.setAttribute('onsubmit', 'addPolicy()');
    policyEditButton.setAttribute('value', 'Add Policy');
    polId.innerHTML = '';
    polCreate.innerHTML = '';
    document.getElementById('policy-title').textContent = 'Add Policy';
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

/* OPEN POLICY EDIT FORM*/
function openPolicyEditForm(policy_id, account_number,
    customer_id, manager_id, policy_type, status, start_date,
    end_date, exposure_amount, created_at, location_id) {

    // Open Modal
    policyModal.classList.add('show-modal');

    // Convert dates to format which type="date" can read
    let start_datef = new Date(start_date).toISOString().slice(0, 10);
    let end_datef = new Date(end_date).toISOString().slice(0, 10);

    // Fill input fields
    document.getElementById('pol_policy_id').value = policy_id;
    document.getElementById('created_at').value = created_at;
    document.getElementById('customer_id').value = customer_id;
    document.getElementById('account_number').value = account_number;
    document.getElementById('manager_id').value = manager_id;
    document.getElementById('policy_type').value = policy_type;
    document.getElementById('policy_status').value = status;
    document.getElementById('start_date').value = start_datef;
    document.getElementById('end_date').value = end_datef;
    document.getElementById('exposure_amount').value = exposure_amount;
    document.getElementById('location_id').value = location_id;

    // Set attributes to PUT
    policyInputForm.setAttribute('method', 'PUT');
    policyInputForm.setAttribute('onsubmit', 'editPolicy()');
    policyEditButton.setAttribute('value', 'Edit Policy');
    document.getElementById('policy-title').textContent = 'Edit Policy';

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

        let policyArray = []; // Create policy ID array for memos + recs to reference

        // Populate policies table with data
        const table = document.querySelector('#policy-table tbody');
        table.innerHTML = ''; // Clear existing rows
        for (const policy of policies) {
            const claim_ratio = await getClaimsCounter(policy.policy_id);
            policy.claim_ratio = claim_ratio;
            policyArray.push(policy.policy_id); // Add policy to array
            const row = document.createElement('tr');
            

            row.innerHTML = `
            <td>${policy.customer_name}</td>
            <td>${policy.policy_id}</td>
            <td>${policy.account_number}</td>
            <td>${policy.status}</td>
            <td>${policy.policy_type}</td>
            <td>${policy.manager_name}</td>
            <td>$${policy.exposure_amount} USD</td>
            <td class="claims-link" data-policy="" onclick="getClaims(${policy.policy_id})">${claim_ratio}</td>
            <td><button id="edit-policy-btn" class="edit-item-btn" onclick='openPolicyEditForm(${policy.policy_id}, ${JSON.stringify(policy.account_number)}, 
                ${policy.customer_id}, ${policy.manager_id}, ${JSON.stringify(policy.policy_type)}, ${JSON.stringify(policy.status)}, ${JSON.stringify(policy.start_date)}, 
                ${JSON.stringify(policy.end_date)}, ${policy.exposure_amount}, ${JSON.stringify(policy.created_at)}, ${policy.location_id}
            )'>Edit</button>
            
            `;
            table.appendChild(row);
        };
        // Delete Policy Button (Removed for Security)
        // <button id="delete-pol-btn" class="delete-item-btn" onclick="deletePolicy(${policy.policy_id})">Delete</button></td>

        // Clear policy ID fields for Recommendation and Memo modals 
        recPolicyIdField.innerText = '';
        memoPolicyIdField.innerText = '';
        // Populate Add Recommendation and Memo Forms with policies
        for (const policy of policies) {
            const recRow = document.createElement('option');
            const memoRow = document.createElement('option');

            recRow.value = policy.policy_id;
            recRow.textContent = policy.policy_id;
            recPolicyIdField.appendChild(recRow);

            memoRow.value = policy.policy_id;
            memoRow.textContent = policy.policy_id;
            memoPolicyIdField.appendChild(memoRow);
        };

        // Save policy array to session storage
        sessionStorage.setItem('policyArray', JSON.stringify(policyArray));

        // Send Customer ID and policy address to New Policy form
        document.getElementById('customer_id').value = policies[0].customer_id;
        document.getElementById('location_id').value = policies[0].location_id;

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

/* ADD NEW POLICY */
async function addPolicy() {

    const add_created_at = new Date(Date.now() - 5 * 60 * 60 * 1000);

    const policy = {
        account_number: document.getElementById('account_number').value,
        customer_id: Number(document.getElementById('customer_id').value),
        manager_id: Number(document.getElementById('manager_id').value),
        policy_type: document.getElementById('policy_type').value,
        status: document.getElementById('policy_status').value,
        start_date: document.getElementById('start_date').value,
        end_date: document.getElementById('end_date').value,
        exposure_amount: Number(document.getElementById('exposure_amount').value),
        created_at: add_created_at,
        location_id: Number(document.getElementById('location_id').value),
    };
    console.log('ID from localStorage:', localStorage.getItem('customer_id'));  
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
                alert('Unable to add policy. ');
                throw new Error(backError);
            }
            return response.json();
        })
        .then(form_data => {
            alert('New policy ID: ' + form_data.policy_id);
            closePolicyForm();
            getPolicies(localStorage.getItem('location_id'));
        })
        .catch(error => {
            console.error('Unable to add policy.', error);
        })
}

/* EDIT POLICY */
async function editPolicy() {

    const policy_id = document.getElementById('pol_policy_id').value;

    const policy = {
        policy_id: Number(document.getElementById('pol_policy_id').value),
        account_number: document.getElementById('account_number').value,
        customer_id: Number(document.getElementById('customer_id').value),
        manager_id: document.getElementById('manager_id').value,
        policy_type: document.getElementById('policy_type').value,
        status: document.getElementById('policy_status').value,
        start_date: document.getElementById('start_date').value,
        end_date: document.getElementById('end_date').value,
        exposure_amount: document.getElementById('exposure_amount').value,
        created_at: document.getElementById('created_at').value,
        location_id: Number(document.getElementById('location_id').value),
    };
    console.log('Submission:', JSON.stringify(policy));
    fetch(`https://localhost:7288/api/policies/${policy_id}`, {
        method: 'PUT',
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
                alert('Unable to edit policy.');
                throw new Error(backError);
            }
            return response.json();
        })
        .then(form_data => {
            alert('Edited policy ID: ' + form_data.policy_id);
            // Set attributes to POST
            closePolicyForm();
            getPolicies(localStorage.getItem('location_id'));
        })
        .catch(error => {
            console.error('Unable to edit policy.', error);
        })
}

/* RECOMMENDATION MODAL FORM */

const addRecButtons = document.querySelectorAll(".add-rec-btn");
const recModal = document.getElementById("addRecModal");
const recInputForm = document.getElementById("addRecForm");
const recEditButton = document.querySelector('#submit_rec_btn');
const recEditor = document.getElementById('rec-editor');
const recPolicyIdField = document.getElementById('rec_policy_id');
const recId = document.createElement('div');

function openRecForm() {
    recModal.classList.add('active');
}
function closeRecForm() {
    recModal.classList.remove('active');
    document.getElementById('addRecForm').reset();
    document.getElementById('rec-editor').innerHTML = '';
    recId.innerHTML = '';

    // Remove edit-only properties
    recInputForm.setAttribute('method', 'POST');
    recInputForm.setAttribute('onsubmit', 'addRecommendation()');
    recEditButton.setAttribute('value', 'Add Recommendation');
    document.getElementById('rec-title').textContent = 'Add Recommendation';
}
function openRecEditForm(recommendation_id, rec_policy_id, recommendation_text) {
    recModal.classList.add('active');

    document.getElementById('recommendation_id').value = recommendation_id;
    document.getElementById('rec_policy_id').value = rec_policy_id;
    document.getElementById('rec-editor').innerHTML = recommendation_text;

    recInputForm.setAttribute('method', 'PUT');
    recInputForm.setAttribute('onsubmit', 'editRecommendation()');
    recEditButton.setAttribute('value', 'Edit Recommendation');
    document.getElementById('rec-title').textContent = 'Edit Recommendation';
}

/* GET RECOMMENDATIONS */
async function getRecommendations() {
    try {
        const grid = document.querySelector('#rec-grid');
        const policyArray = JSON.parse(sessionStorage.getItem('policyArray'));
        grid.innerHTML = ''; // Clear existing grid

        for (let i = 0; i < policyArray.length; i++) {
            const response = await fetch(`https://localhost:7288/api/recommendations/search?policy_id=${policyArray[i]}`);

            // Return error if response fails
            if (!response.ok) {
                const backError = await response.text();
                alert(backError);
                alert('Unable to get claim.');
                throw new Error(backError);
            }

            // Assign JSON object in a variable
            const recommendations = await response.json();

            // Populate recommendations table with data
            recommendations.forEach(recommendation => {
                const box = document.createElement('div');
                const date = new Date(recommendation.created_at).toDateString('en-US');
                box.innerHTML = `
             <div class="box">
             <button id="edit-rec-btn" class="edit-item-btn" onclick='openRecEditForm(${recommendation.recommendation_id}, ${recommendation.policy_id}, ${JSON.stringify(recommendation.recommendation_text)})'>Edit</button>
             <button id="delete-rec-btn" class="delete-item-btn" onclick="deleteRecommendation(${recommendation.recommendation_id})">Delete</button>
             <p>Policy ID: ${recommendation.policy_id}</p>
             <p>Date: ${date}</p>
             <p>${recommendation.recommendation_text}</p>
             </div>
             `;
                grid.appendChild(box);
            });
        }
    } catch (error) {
        console.error(`Error: ${error}`);
    }
}

/* ADD NEW RECOMMENDATION */
async function addRecommendation() {

    const add_created_at = new Date(Date.now() - 5 * 60 * 60 * 1000);
    const user_id = await getUserId();

    const recommendation = {
        user_id: user_id,
        policy_id: document.getElementById('rec_policy_id').value,
        recommendation_text: document.getElementById('rec-editor').innerHTML.trim(),
        created_at: add_created_at,
    };
    console.log('Submission:', JSON.stringify(recommendation));  
    fetch('https://localhost:7288/api/recommendations/', {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(recommendation)
    })
        .then(async response => {  
            if (!response.ok) {
                const backError = await response.text();
                alert(backError);
                alert('Unable to add recommendation. ');
                throw new Error(backError);
            }
            return response.json();
        })
        .then(form_data => {
            alert('New recommendation ID: ' + form_data.recommendation_id);
            closeRecForm();
            getRecommendations();
        })
        .catch(error => {
            console.error('Unable to add recommendation.', error);
        })
}

/* EDIT RECOMMENDATION */
async function editRecommendation() {

    const add_created_at = new Date(Date.now() - 5 * 60 * 60 * 1000);
    const recommendation_id = document.getElementById('recommendation_id').value;
    const user_id = await getUserId();

    const recommendation = {
        user_id: user_id,
        policy_id: document.getElementById('rec_policy_id').value,
        recommendation_text: document.getElementById('rec-editor').innerHTML.trim(),
        created_at: add_created_at,
    };
    console.log('Submission:', JSON.stringify(recommendation));  
    fetch(`https://localhost:7288/api/recommendations/${recommendation_id}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(recommendation)
    })
        .then(async response => {  
            if (!response.ok) {
                const backError = await response.text();
                alert(backError);
                alert('Unable to edit recommendation. ');
                throw new Error(backError);
            }
            return response.json();
        })
        .then(form_data => {
            alert('Edited recommendation ID: ' + form_data.recommendation_id);
            closeRecForm();
            getRecommendations();
        })
        .catch(error => {
            console.error('Unable to edit recommendation.', error);
        })
}

/* DELETE RECOMMENDATION */
async function deleteRecommendation(recommendation_id) {
    if (!recommendation_id) {
        alert('No recommendation ID found.');

        return;
    }

    if (confirm('Are you sure you want to delete the recommendation?')) {
        try {
            const response = await fetch(`https://localhost:7288/api/recommendations/${recommendation_id}`, {
                method: 'DELETE',
                headers: {
                    'Accept': 'application/json',
                },
            })
                .then(() => {
                    alert('Recommendation deleted.');
                    getRecommendations();
                })
        } catch (error) {
            console.error(`Error: ${error}`);
        }
    } else {
        return false;
    }
}

/* MEMO MODAL FORM */
const addMemoButtons = document.querySelectorAll(".add-memo-btn");
const memoModal = document.getElementById("addMemoModal");
const memoInputForm = document.getElementById("addMemoForm");
const memoEditButton = document.querySelector('#submit_memo_btn');
const memoEditor = document.getElementById('memo-editor');
const memoPolicyIdField = document.getElementById('memo_policy_id');
const memoId = document.createElement('div');

function openMemoForm() {
    memoModal.classList.add('active');
}
function closeMemoForm() {
    memoModal.classList.remove('active');
    document.getElementById('addMemoForm').reset();
    document.getElementById('memo-editor').innerHTML = '';
    memoId.innerHTML = '';

    // Remove edit-only properties
    memoInputForm.setAttribute('method', 'POST');
    memoInputForm.setAttribute('onsubmit', 'addMemo()');
    memoEditButton.setAttribute('value', 'Add Memo');
    document.getElementById('memo-title').textContent = 'Add Memo';
}
function openMemoEditForm(memo_id, memo_policy_id, memo_text) {
    memoModal.classList.add('active');

    document.getElementById('memo_id').value = memo_id;
    document.getElementById('memo_policy_id').value = memo_policy_id;
    document.getElementById('memo-editor').innerHTML = memo_text;

    memoInputForm.setAttribute('method', 'PUT');
    memoInputForm.setAttribute('onsubmit', 'editMemo()');
    memoEditButton.setAttribute('value', 'Edit Memo');
    document.getElementById('memo-title').textContent = 'Edit Memo';
}

/* GET MEMOS */
async function getMemos() {
    try {
        const table = document.querySelector('#generalMemoList');
        const policyArray = JSON.parse(sessionStorage.getItem('policyArray'));
        table.innerHTML = ''; // Clear existing grid

        for (let i = 0; i < policyArray.length; i++) {
            const response = await fetch(`https://localhost:7288/api/memos/search?policy_id=${policyArray[i]}`);

            // Return error if response fails
            if (!response.ok) {
                const backError = await response.text();
                alert(backError);
                alert('Unable to get claim.');
                throw new Error(backError);
            }

            // Assign JSON object in a variable
            const memos = await response.json();

            // Populate memos table with data
            memos.forEach(memo => {
                const card = document.createElement('div');
                const date = new Date(memo.created_at).toDateString('en-US');
                card.className = "memo-card";
                card.innerHTML = `
             <div class="memo-info">
             <button id="edit-memo-btn" class="edit-item-btn" onclick='openMemoEditForm(${memo.memo_id}, ${memo.policy_id}, ${JSON.stringify(memo.memo_text)})'>Edit</button>
             <button id="delete-memo-btn" class="delete-item-btn" onclick="deleteMemo(${memo.memo_id})">Delete</button>
             <p>Policy ID: ${memo.policy_id}</p>
             <p>Date: ${date}</p>
             <p>${memo.memo_text}</p>
             </div>
             `;
                table.appendChild(card);
            });
        }
    } catch (error) {
        console.error(`Error: ${error}`);
    }
}

/* ADD NEW MEMO */
async function addMemo() {

    const add_created_at = new Date(Date.now() - 5 * 60 * 60 * 1000);
    const user_id = await getUserId();

    const memo = {
        user_id: user_id,
        policy_id: document.getElementById('memo_policy_id').value,
        memo_text: document.getElementById('memo-editor').innerHTML.trim(),
        created_at: add_created_at,
    };
    console.log('Submission:', JSON.stringify(memo)); // Debug
    fetch('/api/memos', {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(memo)
    })
        .then(async response => {  
            if (!response.ok) {
                const backError = await response.text();
                alert(backError);
                alert('Unable to add memo. ');
                throw new Error(backError);
            }
            return response.json();
        })
        .then(form_data => {
            alert('New memo ID: ' + form_data.memo_id);
            closeMemoForm();
            getMemos();
        })
        .catch(error => {
            console.error('Unable to add memo.', error);
        })
}

/* EDIT MEMO */
async function editMemo() {

    const add_created_at = new Date(Date.now() - 5 * 60 * 60 * 1000);
    const memo_id = document.getElementById('memo_id').value;
    const user_id = await getUserId();

    const memo = {
        user_id: user_id,
        policy_id: document.getElementById('memo_policy_id').value,
        memo_text: document.getElementById('memo-editor').innerHTML.trim(),
        created_at: add_created_at,
    };
    console.log('Submission:', JSON.stringify(memo)); // Debug
    fetch(`https://localhost:7288/api/memos/${memo_id}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(memo)
    })
        .then(async response => {  
            if (!response.ok) {
                const backError = await response.text();
                alert(backError);
                alert('Unable to edit memo. ');
                throw new Error(backError);
            }
            return response.json();
        })
        .then(form_data => {
            alert('Edited memo ID: ' + form_data.memo_id);
            closeMemoForm();
            getMemos();
        })
        .catch(error => {
            console.error('Unable to edit memo.', error);
        })
}

/* DELETE MEMO*/
async function deleteMemo(memo_id) {
    if (!memo_id) {
        alert('No memo ID found.');

        return;
    }

    if (confirm('Are you sure you want to delete the memo?')) {
        try {
            const response = await fetch(`https://localhost:7288/api/memos/${memo_id}`, {
                method: 'DELETE',
                headers: {
                    'Accept': 'application/json',
                },
            })
                .then(() => {
                    alert('Memo deleted.');
                    getMemos();
                });
        } catch (error) {
            console.error(`Error: ${error}`);
        }
    } else {
        return false;
    }
}

/* RECOMMENDATION / MEMOS LISTENER FOR RICH TEXT & MODAL OPEN */
document
    .querySelectorAll(".text-toolbar button[data-command]")
    .forEach((btn) => {
        btn.addEventListener("click", () => {
            const command = btn.getAttribute("data-command");
            document.execCommand(command, false, null);
        });
    });

addRecButtons.forEach(
    (btn) =>
    (btn.onclick = () => {
        recInputForm.reset();
        recModal.classList.add("active");
    })
)

addMemoButtons.forEach(
    (btn) =>
    (btn.onclick = () => {
        memoInputForm.reset();
        memoModal.classList.add("active");
    })
);

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

        // Populate policies table with data
        for (const employee of employees) {
            const claimRow = document.createElement('option');
            const policyRow = document.createElement('option');

            claimRow.value = employee.employee_id;
            claimRow.textContent = employee.name;
            claimAssignedTo.appendChild(claimRow);

            policyRow.value = employee.employee_id;
            policyRow.textContent = employee.name;
            policyAssignedTo.appendChild(policyRow);
        };

    } catch (error) {
        console.error(`Error: ${error}`);
    }
}

/* GET CUSTOMER ID */
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
            console.warn('Parameters were not found!');
        }

        return customer_id;
        // Catch error
    } catch (error) {
        console.error(`Error: ${error}`);
    }
}

document.addEventListener("DOMContentLoaded", () => {
});

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

/* SHARE LOCATION BUTTON */
const shareBtn = document.querySelector(".share-btn");
if (shareBtn) {
    shareBtn.addEventListener("click", async () => {
        const locationURL = 'https://localhost:7288/Resources/customer-resource.html?customer_id=' + localStorage.getItem('customer_id') + '&location_id=' + localStorage.getItem('location_id');
        try {
            await navigator.clipboard.writeText(locationURL);
            alert("📍 Location info copied to clipboard!");
        } catch (err) {
            console.error("Clipboard failed", err);
            alert("Could not copy location link.");
        }
    });
}

/* RECOMMENDATION / MEMOS TABS */
const subTabs = document.querySelectorAll(".sub");
const subContents = document.querySelectorAll(".sub-content");

subTabs.forEach((tab) => {
    tab.addEventListener("click", () => {
        // Remove active class from all
        subTabs.forEach((t) => t.classList.remove("active"));
        subContents.forEach((c) => c.classList.remove("active"));

        // Add active class to the clicked one
        tab.classList.add("active");
        const targetId = tab.dataset.tab;
        const targetContent = document.getElementById(targetId);

        if (targetContent) {
            targetContent.classList.add("active");
        }
    });
});

/* TAB SELECTION */
document.querySelectorAll(".sub").forEach((btn) => {
    btn.addEventListener("click", () => {
        document
            .querySelectorAll(".sub")
            .forEach((b) => b.classList.remove("active"));
        document
            .querySelectorAll(".sub-content")
            .forEach((tab) => tab.classList.remove("active"));
        btn.classList.add("active");
        document.getElementById(btn.dataset.tab).classList.add("active");
    });
});

/* SIDEBAR */
const sidebar = document.getElementById("sidebar-nav");
const toggleButton = document.getElementById("toggle-btn");
const modal = document.getElementById("user-modal");
const profilePicButton = document.getElementById("profile-picture");
const usernameHeader = document.getElementById("username-topbar");


if (localStorage.getItem("sidebarState") === "closed") {
    sidebar.classList.add("close", "reload");
}

setTimeout(() => {
        sidebar.classList.remove("reload");
}, 60);


function toggleSidebar() {
    
    sidebar.classList.toggle("close");
    toggleButton.classList.toggle("rotate");

    if(sidebar.classList.contains("close")) {
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
    for(let i = 0; i < 3; i++) {
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
    // Clear console.
    console.clear();

    // Get Location ID
    const location_id = await getLocation();
    if (!location_id) return;

    // Load rest of data.
    await getPolicies(localStorage.getItem('location_id'));
    await getRecommendations();
    await getMemos();
    await getEmployees();
};


