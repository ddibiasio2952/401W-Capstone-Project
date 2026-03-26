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


modal.onmouseleave = () => {
    modal.classList.remove("active");
}

document.addEventListener("DOMContentLoaded", async () => {
    await releaseToSidebar();
    await usernameToHeader();
});