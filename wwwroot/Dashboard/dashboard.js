// ---------- DATA FETCHING ----------
async function fetchJson(url) {
  try {
    const res = await fetch(url);
    if (!res.ok) throw new Error(res.status);
    return await res.json();
  } catch (err) {
    console.error(`Error fetching ${url}:`, err);
    return [];
  }
}

// Normalizes claim objects so dashboard can display correct fields
function normalizeClaim(c) {
  return {
    claimNumber:
      c.claimNumber || c.claimNo || c.ClaimNumber || c.claim_id || "N/A",
    insured:
      c.insured ||
      c.insuredName ||
      c.customerName ||
      c.clientName ||
      c.InsuredName ||
      "Unknown",
    status: c.status || c.Status || "N/A",
    opened:
      c.dateReported ||
      c.createdDate ||
      c.DateReported ||
      c.date_reported ||
      c.DateOfLoss ||
      null,
    assigned:
      c.assignedTo || c.assigned_employee || c.AssignedTo || "Unassigned",
  };
}

// ---------- PAGE INIT ----------
document.addEventListener("DOMContentLoaded", loadDashboard);

async function loadDashboard() {
  const customers = await fetchJson("https://localhost:7288/api/customers");
  const claims = await fetchJson("https://localhost:7288/api/dashboard/claims-summary");

  renderMetrics(customers, claims); // ✔ removed bad extra comma
  renderRecentActivity(claims);
  renderClaimsTable(claims);
}

// ---------- METRICS (updated to 2 parameters only) ----------
function renderMetrics(customers, claims) {
  document.getElementById("metricActiveClients").textContent =
    customers.length || 0;

  document.getElementById("metricPolicies").textContent = 0; // Safe placeholder

  const openClaims = claims.filter(
    (c) => (c.status || "").toLowerCase() === "open"
  );
  document.getElementById("metricOpenClaims").textContent = openClaims.length;

  // Avg claim age
  const validDates = claims
    .map((c) => normalizeClaim(c).opened)
    .filter((date) => date && !isNaN(new Date(date)));

  if (validDates.length === 0) {
    document.getElementById("metricClaimAge").textContent = "N/A";
    return;
  }

  const ages = validDates.map((date) =>
    Math.floor((Date.now() - new Date(date)) / 86400000)
  );

  document.getElementById("metricClaimAge").textContent =
    Math.round(ages.reduce((a, b) => a + b, 0) / ages.length) + " days";
}

// ---------- RECENT ACTIVITY ----------
function renderRecentActivity(claims) {
  const list = document.getElementById("recentList");
  list.innerHTML = "";

  if (!claims || claims.length === 0) {
    list.innerHTML = "<li>No recent activity</li>";
    return;
  }

  claims.slice(0, 5).forEach((raw) => {
    const c = normalizeClaim(raw);
    const li = document.createElement("li");
    li.innerHTML = `<strong>#${c.claimNumber}</strong> — ${c.status} (${c.insured})`;
    list.appendChild(li);
  });
}

// ---------- CLAIMS PREVIEW TABLE ----------
function renderClaimsTable(claims) {
  const body = document.getElementById("claimsPreviewBody");
  body.innerHTML = "";

  const openClaims = claims
    .filter((c) => (c.status || "").toLowerCase() === "open")
    .slice(0, 5);

  if (openClaims.length === 0) {
    body.innerHTML = `<tr><td colspan="4">No open claims</td></tr>`;
    return;
  }

  openClaims.forEach((raw) => {
    const c = normalizeClaim(raw);
    const displayDate =
      c.opened && !isNaN(new Date(c.opened))
        ? new Date(c.opened).toLocaleDateString()
        : "N/A";

    const tr = document.createElement("tr");
    tr.innerHTML = `
      <td>${c.claimNumber}</td>
      <td>${c.insured}</td>
      <td>${c.status}</td>
      <td>${displayDate}</td>
    `;
    body.appendChild(tr);
  });
}
