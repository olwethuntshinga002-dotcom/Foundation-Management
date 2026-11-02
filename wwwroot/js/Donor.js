// Sidebar behaviour for the donor layout.
// NOTE: the class names used for collapsing the sidebar must match the ones
// in DashStaff.css / DashStaff.js. If your sidebar toggle uses a different
// class, change SIDEBAR_CLASS below.
(function () {
    "use strict";

    var SIDEBAR_CLASS = "sidebar-open"; // <-- match DashStaff.js if different

    // Expand / collapse submenu groups
    document.querySelectorAll(".menu-header").forEach(function (header) {
        header.addEventListener("click", function () {
            var group = header.closest(".menu-group");
            if (group) group.classList.toggle("open");
        });
    });

    // Hamburger button toggles the sidebar
    var button = document.getElementById("navigationButton");
    var sidebar = document.getElementById("sidebar");

    if (button && sidebar) {
        button.addEventListener("click", function () {
            sidebar.classList.toggle(SIDEBAR_CLASS);
            document.body.classList.toggle(SIDEBAR_CLASS);
        });
    }
})();