/* DAN EDIT 11/29/2025 PM */
/*
    --------------------
        AN's Work
    --------------------
*/

const signinBox = document.getElementById("signin-box");
const signupBox = document.getElementById("signup-box");


// Functions to switch between sign up and sign in forms;
document.getElementById("go-signup").onclick = () => {
    signinBox.classList.add("hidden");
    signupBox.classList.remove("hidden");
};

document.getElementById("go-signin").onclick = () => {
    signupBox.classList.add("hidden");
    signinBox.classList.remove("hidden");
};

/*
    --------------------
        End of AN's Work
    --------------------
*/


// Redirects user to dashboard with a new session
async function signIn(form) {
    const email = form.email.value;
    const password = form.password.value;


    // Validates user credentials in backend
    const response = await fetch(`https://localhost:7288/api/auth/login?email=${encodeURIComponent(email)}&password=${encodeURIComponent(password)}`, {
        method: "POST",
        credentials: "include"
    });

    if (!response.ok) {
        const errorMsg = await response.text();
        alert(errorMsg);
        console.warn(errorMsg);
    }

    // Alerts that login was a success
    const result = await response.text();
    alert(result);

    // Redirect to dashboard
    window.location.href = "https://localhost:7288/home/dashboard";
}

// Registers user to backend
async function signUp(form) {
    if (form.password.value !== form.confirm.value) {
        alert("Passwords do not match");
        return;
    }
    const email = form.email.value;
    const password = form.password.value;
    const employee_id = form.emp_id.value;

    const response = await fetch(`https://localhost:7288/api/account/register?email=${encodeURIComponent(email)}&password=${encodeURIComponent(password)}&id=${encodeURIComponent(employee_id)}`, {
        method: "POST"
    });

    if (!response.ok) {
        const errorMsg = await response.text();
        alert(errorMsg);
        console.warn(errorMsg);
    }

    // Alerts the sign up was a success
    const result = await response.text();
    alert(result);
}

// User selects submit button to sign in
signinBox.addEventListener("submit", async (event) => {
    event.preventDefault();

    const form = event.target;
    await signIn(form);
});

// User selects submit button to sign up
signupBox.addEventListener("submit", async (event) => {
    event.preventDefault();

    const form = event.target;
    await signUp(form);
});
