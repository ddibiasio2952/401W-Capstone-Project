// ---------- DATA FETCH ----------
async function fetchJson(url) {
  try {
    const res = await fetch(url);
    if (!res.ok) throw new Error(res.status);
    return await res.json();
  } catch (err) {
    console.error("Fetch error:", url, err);
    return null;
  }
}

// ---------- PAGE INIT ----------
document.addEventListener("DOMContentLoaded", loadClaims);
let allClaims = [];

// ---------- LOAD CLAIMS ----------
async function loadClaims() {
  const claims = await fetchJson("https://localhost:7288/api/claims");
  const summaries = await fetchJson("https://localhost:7288/api/dashboard/claims-summary");

  allClaims = claims.map((c) => {
    const m = summaries.find((s) => s.claimId === c.claim_id);
    return {
      claimId: c.claim_id,
      claimNumber: c.claim_number || `CLM-${c.claim_id}`,
      status: c.status || "Unknown",
      policyId: c.policy_id,
      locationId: c.claim_policy.location_id,
      dateReported: c.date_reported,
      insuredName: m ? m.insuredName : "Unknown",
      customerId: m ? m.customerId : null,
    };
  });

  renderClaimsTable(allClaims);
  setupFilter();
  setupSearch();
}

// ---------- RENDER CLAIMS TABLE ----------
function renderClaimsTable(claims) {
  const body = document.getElementById("claimsTableBody");
  body.innerHTML = "";

  if (!claims.length) {
    body.innerHTML = `<tr><td colspan="6">No claims found</td></tr>`;
    return;
  }

  claims.forEach((c) => {
    const tr = document.createElement("tr");
    tr.style.cursor = c.customerId ? "pointer" : "default";

    tr.innerHTML = `
      <td><span class="clickable">${c.claimNumber}</span></td>
      <td><span class="clickable">${c.insuredName}</span></td>
      <td>${c.policyId}</td>
      <td>${getStatusBadge(c.status)}</td>
      <td>${formatDate(c.dateReported)}</td>
      <td><button class="claim-button" onclick="event.stopPropagation(); viewClaim(${c.claimId}, ${
      c.policyId
    }, ${c.customerId})">View</button></td>
    `;

    console.log("CLAIM ***:", c);

    if (c.locationId) {
      tr.addEventListener("click", () => {
        window.location.href = `../Location/customer-location.html?location_id=${c.locationId}`;
      });
    }

    body.appendChild(tr);
  });
}

// ---------- VIEW CLAIM DETAILS (CLEAN DOM VERSION) ----------
async function viewClaim(claimId, policyId, customerId) {
  const claim = await fetchJson(`https://localhost:7288/api/claims/${claimId}`);
  const policy = await fetchJson(`https://localhost:7288/api/policies/${policyId}`);
  const customer = await fetchJson(`https://localhost:7288/api/customers/${customerId}`);
  const notes = await fetchJson(`https://localhost:7288/api/claimnotes/${claimId}`);

  const modalBody = document.getElementById("modalBody");
  modalBody.innerHTML = ""; // Clear previous content

  // --- Claim Header ---
  modalBody.appendChild(createTitle(`Claim #${claim.claim_number}`));
  modalBody.appendChild(createField("Status", claim.status));
  modalBody.appendChild(
    createField("Date Reported", formatDate(claim.date_reported))
  );
  modalBody.appendChild(
    createField("Date of Loss", formatDate(claim.date_of_loss))
  );
  modalBody.appendChild(createDivider());

  // --- Policy Section ---
  modalBody.appendChild(createSection("Policy Information"));
  modalBody.appendChild(createField("Policy ID", policy.policy_id));
  modalBody.appendChild(createField("Policy Type", policy.policy_type));
  modalBody.appendChild(createField("Coverage", policy.coverage));
  modalBody.appendChild(
    createField("Expiration", formatDate(policy.expiration_date))
  );
  modalBody.appendChild(createDivider());

  // --- Customer Section ---
  modalBody.appendChild(createSection("Customer Information"));
  modalBody.appendChild(createField("Name", customer.name));
  modalBody.appendChild(createField("Email", customer.email));
  modalBody.appendChild(createField("Phone", customer.phone));
  modalBody.appendChild(
    createButton("Go to Customer Page", () => {
      window.location.href = `../Location/customer-location.html?location_id=${policy.location_id}`;
    })
  );
  modalBody.appendChild(createDivider());

  // --- Claim Notes ---
  modalBody.appendChild(createSection("Claim Notes"));
  if (notes?.length) {
    notes.forEach((n) => {
      modalBody.appendChild(createNote(n.note_text, formatDate(n.date_added)));
    });
  } else {
    modalBody.appendChild(createField("Notes", "No notes available"));
  }

  openModal();
}

// ---------- STATUS BADGE ----------
function getStatusBadge(status) {
  const s = status.toLowerCase();
  let className = "badge-default";
  if (s === "open") className = "badge-open";
  else if (s === "closed") className = "badge-closed";
  else if (s === "pending") className = "badge-pending";

  return `<span class="badge ${className}">${status}</span>`;
}

// ---------- DATE FORMAT ----------
function formatDate(date) {
  return date && !isNaN(new Date(date))
    ? new Date(date).toLocaleDateString()
    : "N/A";
}

// ---------- FILTER ----------
function setupFilter() {
  document.getElementById("statusFilter").addEventListener("change", (e) => {
    const value = e.target.value.toLowerCase();
    const filtered =
      value === "all"
        ? allClaims
        : allClaims.filter((c) => c.status.toLowerCase() === value);
    renderClaimsTable(filtered);
  });
}

// ---------- SEARCH ----------
function setupSearch() {
  document.getElementById("searchBar").addEventListener("input", (e) => {
    const term = e.target.value.toLowerCase();
    const filtered = allClaims.filter(
      (c) =>
        c.claimNumber.toLowerCase().includes(term) ||
        c.insuredName.toLowerCase().includes(term)
    );
    renderClaimsTable(filtered);
  });
}

// ---------- MODAL HANDLING ----------
function openModal() {
  document.getElementById("claimModal").classList.remove("hidden");
}
document.getElementById("closeModal").addEventListener("click", () => {
  document.getElementById("claimModal").classList.add("hidden");
});
window.addEventListener("click", (e) => {
  if (e.target.classList.contains("modal")) {
    document.getElementById("claimModal").classList.add("hidden");
  }
});

// ---------- DOM HELPER FUNCTIONS ----------
function createTitle(text) {
  const h = document.createElement("h2");
  h.textContent = text;
  return h;
}

function createSection(text) {
  const h = document.createElement("h3");
  h.style.marginTop = "10px";
  h.textContent = text;
  return h;
}

function createField(label, value) {
  const p = document.createElement("p");
  p.innerHTML = `<strong>${label}:</strong> ${value || "N/A"}`;
  return p;
}

function createDivider() {
  return document.createElement("hr");
}

function createButton(text, onClick) {
  const btn = document.createElement("button");
  btn.classList.add("claim-button");
  btn.textContent = text;
  btn.onclick = onClick;
  btn.style.margin = "5px";
  return btn;
}

function createNote(text, date) {
  const div = document.createElement("div");
  div.style.marginBottom = "5px";
  div.innerHTML = `<em>${date}</em>: ${text}`;
  return div;
}
