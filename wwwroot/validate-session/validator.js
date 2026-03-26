	CheckSessionStorage(); 
	setInterval(CheckEveryNthMin, 1000);

	
	// This function checks the session's state every Nth minute to see if it's still valid
	async function CheckEveryNthMin() 
	{
		let now = Date.now();
		let LATER_TIME = parseInt(sessionStorage.getItem('time'), 10); // fetches stored timestamp

		// checks if assigned minute has passed
		if (now >= LATER_TIME) {
			LATER_TIME += 2 * 60 * 1000;
			sessionStorage.setItem('time', LATER_TIME); // updates timestamp to another [num] minutes

			await SetBackEndTime(LATER_TIME);
			await CheckSessionValidity(); // checks if session is still valid
		}


	}

	// This function checks a session's validity
	// If invalid, it takes the neccessary actions
	async function CheckSessionValidity()
	{
		// checks the session's state via call to backend
		const response =  await fetch("https://localhost:7288/api/sessions/validate", 
		{
			method: "POST",
			credentials: "include"
		});

		// redirects to login if session is invalid
		if (response.status === 401 || response.status === 500)
		{
			alert("Session has expired or is invalid");
			sessionStorage.clear();
			window.location.href = "https://localhost:7288/";
		}

	}

	/* It checks if sessionStorage is empty to set up the intial timestamp.
	 * This function also considers the edge case where someone deletes their
	 * cookies and history via a call to the backend where it checks the session's
	 * state.
	*/
	async function CheckSessionStorage()
	{
		if (!sessionStorage.getItem('time'))
		{
			await CheckSessionValidity();
			let timer_in_ms = await GetBackEndTime();
			sessionStorage.setItem('time', timer_in_ms);

		}
	}

	async function SetBackEndTime(time)
	{
		let time_in_ms = time.toString();

		try
		{
			const response = await fetch("https://localhost:7288/api/sessions/set-timer",
			{
				method: "POST",
				headers: {"Content-Type": "application/json"}, 
				body: JSON.stringify(time_in_ms)
			});

			if (!response.ok)
			{
				throw new Error("Could not set timer in back-end");
			}
		}
		catch (Error)
		{
			console.error(`Error: ${Error}`);
			await CheckSessionValidity();
		}
		


	}

	async function GetBackEndTime()
	{
		try 
		{
			
			const response = await fetch("https://localhost:7288/api/sessions/get-timer",
			{
				method: "GET",
				credentials: "include"
			});

		
			if (!response.ok) 
			{
				throw new Error("Could not find timer in back-end");
			}	

			const timer = await response.json();
			return timer;

		}
		catch (Error) 
		{
			console.error(`Error: ${Error}`);
			await CheckSessionValidity();
		}
	}


	/* 
	 * Fetch ID of logged in user from a session variable if unavailable
	 * Then set it in a browser's session storage for future use
	*/
	async function getUserId() {
	    try {
			let user_id = sessionStorage.getItem('userId');
			if(!user_id) {
				// Fetch from a session variable in backend
				const response = await fetch(`https://localhost:7288/api/Sessions/get-id`);
	
				// Return error if response fails
				if (!response.ok) {
					const backError = await response.text();
					alert(backError);
					alert('Unable to get ID.');
					throw new Error(backError);
				}
	
				// Assign JSON object in a variable
				const userId = await response.json();
				
				// Set in session storage for future use
				sessionStorage.setItem('userId', userId);
				user_id = sessionStorage.getItem('userId');
			}
			
	        return parseInt(user_id, 10);
	    } catch (error) { // Catch error
	        console.error(`Error: ${error}`);
	    }
	}
