document.addEventListener("DOMContentLoaded", function () {

    /* ELEMENT REFERENCES */

    const sidebar = document.getElementById("sidebar");
    const main = document.getElementById("main");
    const navigationButton = document.getElementById("navigationButton");

    const modalOverlay = document.getElementById("modalOverlay");
    const modalForm = document.getElementById("modalForm");
    const modalTitle = document.getElementById("modalTitle");
    const modalSubtitle = document.getElementById("modalSubtitle");
    const modalClose = document.getElementById("modalClose");
    const modalCancel = document.getElementById("modalCancel");

    const donationFields = document.getElementById("donationFields");
    const attendanceFields = document.getElementById("attendanceFields");

    const donationTypeField = document.getElementById("donationType");
    const donationQtyField = document.getElementById("donationQty");
    const attendanceCountField = document.getElementById("attendanceCount");

    const activityList = document.getElementById("activityList");
    const toastStack = document.getElementById("toastStack");

    const statFood = document.getElementById("statFood");
    const statClothing = document.getElementById("statClothing");
    const statBeneficiaries = document.getElementById("statBeneficiaries");

    let activeMode = "donation"; // "donation" | "attendance"

    /* SIDEBAR TOGGLE */

    navigationButton.addEventListener("click", function () {

        if (window.innerWidth <= 700) {
            sidebar.classList.toggle("mobile-open");
        } else {
            sidebar.classList.toggle("closed");
            main.classList.toggle("expanded");
        }

    });

    /* SIDEBAR SUBMENU */

    document.querySelectorAll(".menu-header").forEach(function (header) {
        header.addEventListener("click", function () {
            header.parentElement.classList.toggle("open");
        });
    });

    document.querySelectorAll(".nav-link[data-nav]").forEach(function (link) {
    link.addEventListener("click", function (e) {

        // Only block navigation for placeholder links (href="#").
        // A link pointing at a real page is left to navigate normally.
        if (link.getAttribute("href") === "#") {
            e.preventDefault();
        }

        document.querySelectorAll(".nav-link[data-nav]").forEach(function (l) {
            l.classList.remove("active");
        });

        link.classList.add("active");

        if (window.innerWidth <= 700) {
            sidebar.classList.remove("mobile-open");
        }
    });
});

    /* MODAL HELPERS */

    function openModal(mode) {

        activeMode = mode;
        modalForm.reset();

        if (mode === "donation-food" || mode === "donation-clothing" || mode === "donation") {

            donationFields.hidden = false;
            attendanceFields.hidden = true;

            modalTitle.textContent = "Record a donation";
            modalSubtitle.textContent = "Add newly received items to stock.";

            if (mode === "donation-food") {
                donationTypeField.value = "food";
            } else if (mode === "donation-clothing") {
                donationTypeField.value = "clothing";
            }

            activeMode = "donation";

        } else if (mode === "attendance") {

            donationFields.hidden = true;
            attendanceFields.hidden = false;

            modalTitle.textContent = "Record attendance";
            modalSubtitle.textContent = "Log beneficiaries visiting today.";

            activeMode = "attendance";
        }

        modalOverlay.classList.add("visible");
        modalOverlay.setAttribute("aria-hidden", "false");

        window.setTimeout(function () {
            const firstField = mode === "attendance" ? attendanceCountField : donationQtyField;
            if (firstField) firstField.focus();
        }, 150);
    }

    function closeModal() {
        modalOverlay.classList.remove("visible");
        modalOverlay.setAttribute("aria-hidden", "true");
    }

    modalClose.addEventListener("click", closeModal);
    modalCancel.addEventListener("click", closeModal);

    modalOverlay.addEventListener("click", function (e) {
        if (e.target === modalOverlay) closeModal();
    });

    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape" && modalOverlay.classList.contains("visible")) {
            closeModal();
        }
    });

    /* QUICK ACTION / HEADER TRIGGERS */

    document.querySelectorAll("[data-open-modal]").forEach(function (button) {
        button.addEventListener("click", function () {
            openModal(button.getAttribute("data-open-modal"));
        });
    });

    const viewPledgesButton = document.getElementById("viewPledgesAction");
    if (viewPledgesButton) {
        viewPledgesButton.addEventListener("click", function () {
            const pledgesLink = document.querySelector('.nav-link[data-nav="pledges"]');
            document.querySelectorAll(".nav-link[data-nav]").forEach(function (l) {
                l.classList.remove("active");
            });
            if (pledgesLink) pledgesLink.classList.add("active");
            showToast("Pledges section selected in the sidebar");
        });
    }

    /* TOAST */

    function showToast(message) {
        const toast = document.createElement("div");
        toast.className = "toast";
        toast.textContent = message;
        toastStack.appendChild(toast);

        window.setTimeout(function () {
            toast.classList.add("leaving");
            toast.addEventListener("animationend", function () {
                toast.remove();
            });
        }, 3000);
    }

    /* STAT / ACTIVITY UPDATES */

    function bumpStat(el, amount) {
        const current = parseInt(el.textContent.replace(/,/g, ""), 10) || 0;
        el.textContent = (current + amount).toLocaleString();
        el.classList.add("pulse");
        window.setTimeout(function () {
            el.classList.remove("pulse");
        }, 900);
    }

    function addActivity(icon, title, detail) {
        const item = document.createElement("div");
        item.className = "activity activity-new";
        item.innerHTML =
            '<div class="activity-icon">' + icon + '</div>' +
            '<div class="activity-details">' +
                '<strong>' + title + '</strong>' +
                '<span>' + detail + '</span>' +
            '</div>' +
            '<time>' + formatTime(new Date()) + '</time>';

        activityList.insertBefore(item, activityList.firstChild);

        const emptyState = activityList.querySelector(".empty-state");
        if (emptyState) emptyState.remove();
    }

    function formatTime(date) {
        let hours = date.getHours();
        const minutes = date.getMinutes().toString().padStart(2, "0");
        const suffix = hours >= 12 ? "PM" : "AM";
        hours = hours % 12;
        if (hours === 0) hours = 12;
        return hours + ":" + minutes + " " + suffix;
    }

    /* FORM SUBMIT */

    modalForm.addEventListener("submit", function (e) {
        e.preventDefault();

        if (activeMode === "donation") {

            const type = donationTypeField.value;
            const qty = parseInt(donationQtyField.value, 10);

            if (!qty || qty <= 0) {
                donationQtyField.focus();
                return;
            }

            if (type === "food") {
                bumpStat(statFood, qty);
                addActivity("🍲", "Food donation recorded", qty + " food items were added to stock.");
                showToast(qty + " food item" + (qty === 1 ? "" : "s") + " added to stock");
            } else {
                bumpStat(statClothing, qty);
                addActivity("👕", "Clothing donation received", qty + " clothing items received.");
                showToast(qty + " clothing item" + (qty === 1 ? "" : "s") + " added to stock");
            }

        } else if (activeMode === "attendance") {

            const count = parseInt(attendanceCountField.value, 10);

            if (!count || count <= 0) {
                attendanceCountField.focus();
                return;
            }

            bumpStat(statBeneficiaries, count);
            addActivity("👥", "Beneficiary attendance", count + " beneficiaries recorded today.");
            showToast("Attendance logged for " + count + " beneficiar" + (count === 1 ? "y" : "ies"));
        }

        closeModal();
    });

});