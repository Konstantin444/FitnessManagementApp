document.addEventListener("DOMContentLoaded", function () {
    const reserveModal = document.getElementById("reserveModal");
    const reserveButton = document.getElementById("reserveButton");
    const modalError = document.getElementById("modalError");

    reserveModal.addEventListener("show.bs.modal", function (event) {
        const triggerButton = event.relatedTarget;

        // Extract session data attributes
        const sessionId = triggerButton.getAttribute("data-session-id");
        const sessionName = triggerButton.getAttribute("data-session-name");
        const sessionTrainer = triggerButton.getAttribute("data-session-trainer");
        const sessionDate = triggerButton.getAttribute("data-session-date");
        const maxParticipants = triggerButton.getAttribute("data-max-participants");
        const availableSlots = triggerButton.getAttribute("data-available-slots");
        const isUserReserved = triggerButton.getAttribute("data-is-user-reserved") === "true";

        // Populate modal fields
        document.getElementById("sessionName").textContent = sessionName;
        document.getElementById("sessionTrainer").textContent = sessionTrainer;
        document.getElementById("sessionDate").textContent = sessionDate;
        document.getElementById("maxParticipants").textContent = maxParticipants;
        document.getElementById("availableSlots").textContent = availableSlots;

        // Reset error message
        modalError.style.display = "none";

        // Configure Join button
        reserveButton.style.display = isUserReserved ? "none" : "inline-block";
        reserveButton.onclick = function () {
            toggleReservation(sessionId);
        };
    });

    function toggleReservation(sessionId) {
        // CSRF token
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        fetch(`/TrainingSessions/Reserve/${sessionId}`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": token
            }
        })
            .then(response => {
                if (response.ok) {
                    console.log("Reservation successful for session:", sessionId);
                    reserveModal.querySelector(".btn-close").click(); // Close the modal
                    location.reload(); // Reload the page to reflect changes
                } else {
                    return response.text(); // Read error message from the response
                }
            })
            .then(errorMessage => {
                if (errorMessage) {
                    console.error("Reservation failed:", errorMessage);
                    modalError.textContent = errorMessage; // Display the error in the modal
                    modalError.style.display = "block";
                }
            })
            .catch(error => {
                console.error("Fetch error:", error);
                modalError.textContent = "A network error occurred. Please try again.";
                modalError.style.display = "block";
            });
    }
});
