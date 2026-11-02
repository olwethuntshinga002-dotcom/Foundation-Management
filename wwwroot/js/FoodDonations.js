document.addEventListener("DOMContentLoaded", function () {

    const createdAt = document.getElementById("createdAt");
    const donationDate = document.getElementById("donationDate");
    const successToast = document.getElementById("successToast");

    /* DEFAULTS (display only —
       the real values are set server-side on save) */

    if (createdAt) {
        createdAt.value = formatDateTime(new Date());
    }

    if (donationDate && !donationDate.value) {
        donationDate.valueAsDate = new Date();
    }

    function formatDateTime(date) {
        const pad = function (n) { return n.toString().padStart(2, "0"); };
        return date.getFullYear() + "-" + pad(date.getMonth() + 1) + "-" + pad(date.getDate()) +
            " " + pad(date.getHours()) + ":" + pad(date.getMinutes());
    }

    /* SUCCESS TOAST (rendered by the server after a successful save,
       via TempData — this just fades it out) */

    if (successToast) {
        window.setTimeout(function () {
            successToast.classList.add("leaving");
            successToast.addEventListener("animationend", function () {
                successToast.remove();
            });
        }, 3000);
    }

});