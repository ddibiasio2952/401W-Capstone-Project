const audit_table = document.getElementById("login-audit-table");
const audit_tbody = audit_table.querySelector("tbody");

const session_table = document.getElementById("sessions-table");
const session_tbody = session_table.querySelector("tbody");

const usernameHeader = document.getElementById("username-topbar");


// Displays login data by appending it to login audit table
async function displayLoginAudits() {
    try {

        // Fetches the login audits for the logged in user
        const response = await fetch("https://localhost:7288/api/audit/audits");

        if(!response.ok){
            throw new Error("Failed to fetch login audits")
        }

        const audits = await response.json();

        // Create and render the audit cards to append later
        const rows = await renderLoginAudits(audits);

        // Clear existing rows
        audit_tbody.innerHTML = "";

        // Append login audits to table body to display them
        rows.forEach(audit => {
            audit_tbody.appendChild(audit);
        });
    } 
    catch(errorMsg) {
        console.error(errorMsg);
    }
}

// Creates a table row with login data
async function renderLoginAudits(audits) {
    let event_svg;

    // Reverse audits to have most recent at top
    audits.reverse();
    
    // Creates each audit card concurrently
    const rendered_audits = await Promise.all( 
        audits.map(async (audit) => {

            // Assigns an svg based on login event
            switch(audit.login_event) {
                case "sign in success":
                    // Green check marker
                    event_svg = `
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="#10b981">
                        <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
                        <polyline points="22 4 12 14.01 9 11.01"></polyline>
                    </svg>`;
                    break;
                case "sign in failure":
                    // Red X
                    event_svg = `
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="#ef4444">
                        <circle cx="12" cy="12" r="10"></circle>
                        <line x1="15" y1="9" x2="9" y2="15"></line>
                        <line x1="9" y1="9" x2="15" y2="15"></line>
                    </svg>`;
                    break;
                case "sign out":
                    // Two green vertical lines
                    event_svg = `
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="#10b981">
                        <circle cx="12" cy="12" r="10"></circle>
                        <line x1="9" y1="4" x2="9" y2="20"></line>
                        <line x1="15" y1="4" x2="15" y2="20"></line>
                    </svg>`;
                    break;
                case "signed out single session":
                    // Two orange vertical lines; Signifies when a session is revoked
                    event_svg = `
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="#f77c1a">
                        <circle cx="12" cy="12" r="10"></circle>
                        <line x1="9" y1="4" x2="9" y2="20"></line>
                        <line x1="15" y1="4" x2="15" y2="20"></line>
                    </svg>`;
                    break;
                default:
                    throw new Error("Invalid Event");
            }

            // Creates the audit card that will hold login information
            const audit_card = document.createElement("tr");
            audit_card.classList.add("audit-card");

            // Contents of card: login event, user agent (browser), and IP address
            audit_card.innerHTML = `
                <td>
                    <div class="cell">
                        <div class="inline-status">
                            <span>
                                ${event_svg}
                            </span>
                            <span>${audit.login_event}</span>
                        </div>
                        <time>${audit.occurred_at}</time>
                    </div>
                </td>
                <td>
                    <div class="cell user-agent">
                        ${audit.user_agent}
                    </div>
                </td>
                <td>
                    <div class="cell ip">
                        ${audit.ip_address}
                    </div>
                </td>
            `;
            
            return audit_card;
        }) // end of map function
    );

    return rendered_audits;
}

// Displays session data by appending it to session table
async function displaySessions() {
    try {
        const response = await fetch(`https://localhost:7288/api/sessions/my-sessions`);

        if(!response.ok) {
            throw new Error("Failed to fetch user sessions");
        }

        const sessions = await response.json();
        console.log("SESSIONS:", sessions);

        const [current_session, other_sessions] = await renderSessions(sessions);

        session_tbody.appendChild(current_session);

        other_sessions.forEach(session => {
            session_tbody.appendChild(session);
        });

    }
    catch(errorMsg) {
        console.error(errorMsg);
    }
}

// Creates table row(s) with session data
async function renderSessions(sessions) {
    if (sessions === null || typeof sessions === "undefined") {
        console.warn("No sessions found");
        return;
    }

    const current_session = sessions.current_session;
    const other_sessions = sessions.other_sessions;

    // Creates a card for the current session
    const current_session_card = document.createElement("tr");
    current_session_card.classList.add("session-card");
    if (current_session) {
        current_session_card.innerHTML = `
                <td>
                    <div class="cell user-agent">
                        ${current_session.user_agent}
                    </div>
                </td>
                <td>
                    <div class="cell date">
                        <time>${current_session.created_at}</time>
                    </div>
                </td>
                <td>
                    <div class="cell action">
                        <span class="current-session">Current Session</span>
                    </div>
                </td>`;
    }
    else {
        throw new Error("Current session does not exist!");
    }

    // Concurrently creates cards for other sessions
    // Skips if there are no other sessions besides current
    let other_session_cards = [];
    if(other_sessions.length > 0) {
        other_session_cards = await Promise.all(
            other_sessions.map(async session => {
                const session_card = document.createElement("tr");
                session_card.classList.add("session-card");

                session_card.innerHTML = `
                    <td>
                        <div class="cell user-agent">
                            ${session.user_agent}
                        </div>
                    </td>
                    <td>
                        <div class="cell date">
                            <time>${session.created_at}</time>
                        </div>
                    </td>
                    <td>
                        <div class="cell action">
                            <button class="session-btn" onclick="revokeSession(this, '${session.session_hash}')">
                                <span>Revoke</span>
                            </button>
                        </div>
                    </td>`;
                
                return session_card;
            }) // end of map function
        ); // end of promise
    }

    return [current_session_card, other_session_cards];
}

async function revokeSession(button, hash) {
    try {
        const session_row = button.closest("tr");

        console.log("HASH ***: ", typeof(hash));
        const response = await fetch("https://localhost:7288/api/sessions/revoke", {
            method: "POST",
            headers: {"Content-Type": "application/json"},
            body: JSON.stringify(hash)
        });

        if(!response.ok) {
            throw new Error("Failed to revoke session");
        }

        session_row.remove();
        alert("Session revoked");

        await displayLoginAudits();
    }
    catch(errorMsg) {
        console.error(errorMsg);
    }
    
}

// Side bar

function goBack() {
    history.back();
}


// Header

const modal = document.getElementById("user-modal");
const profilePicButton = document.getElementById("profile-picture");

async function usernameToHeader() {
    try {
        const response = await fetch('https://localhost:7288/api/account/account-name');

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

profilePicButton.onclick = () => {
    modal.classList.toggle('active');
}

modal.onmouseleave = () => {
    modal.classList.remove("active");
}


window.addEventListener("DOMContentLoaded", async () => {
    await usernameToHeader();
    await displaySessions();
    await displayLoginAudits();
});
