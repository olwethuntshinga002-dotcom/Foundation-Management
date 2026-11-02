document.addEventListener("DOMContentLoaded", function () {

    const sidebar =
        document.getElementById("sidebar");

    const main =
        document.getElementById("main");

    const navigationButton =
        document.getElementById("navigationButton");


    /*
     * TOP-LEFT NAVIGATION BUTTON
     *
     * Opens and closes the entire sidebar.
     */

    navigationButton.addEventListener("click", function () {

        if (window.innerWidth <= 700) {

            sidebar.classList.toggle("mobile-open");

        } else {

            sidebar.classList.toggle("closed");

            main.classList.toggle("expanded");

        }

    });


    /* SIDEBAR DROPDOWN MENUS */

    const menuHeaders =
        document.querySelectorAll(".menu-header");


    menuHeaders.forEach(function (header) {

        header.addEventListener("click", function () {

            const group =
                header.parentElement;

            group.classList.toggle("open");

        });

    });

});