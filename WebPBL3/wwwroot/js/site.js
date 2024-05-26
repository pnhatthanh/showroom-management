const tabs = document.querySelectorAll('.nav-item');
const currentPath = window.location.pathname;
tabs.forEach(tab => {
    const tabLink = tab.querySelector('a');
    if (tabLink && tabLink.getAttribute('href') === currentPath) {
        tab.classList.add('active');
    } else {
        tab.classList.remove('active');
    }
});
//document.addEventListener("DOMContentLoaded", function () {
//    $(document).ready(function () {
//        var cars = $(".car-wrap");
//        var carsPerPage = 12;
//        var totalPages = Math.ceil(cars.length / carsPerPage);
//        var visiblePages = 5;
//        showPage(1);
//        function showPage(page) {
//            var startIndex = (page - 1) * carsPerPage;
//            var endIndex = startIndex + carsPerPage;
//            cars.hide().slice(startIndex, endIndex).show();
//        }
//        var pagination = $("#pagination ul");
//        var middlePageIndex = Math.ceil(visiblePages / 2);
//        var startPage = 1;
//        if (totalPages > visiblePages) {
//            startPage = Math.max(1, currentPage - middlePageIndex + 1);
//            if (currentPage > totalPages - middlePageIndex) {
//                startPage = totalPages - visiblePages + 1;
//            }
//        }
//        pagination.prepend(`<li><a href="#" class="page-link">&lt;</a></li>`);

//        for (var i = startPage; i < startPage + visiblePages; i++) {
//            pagination.append(`<li><a href="#" class="page-link">${i}</a></li>`);
//        }
//        pagination.append(`<li><a href="#" class="page-link">&gt;</a></li>`);

//        // Xử lý sự kiện khi nhấp vào một liên kết trang
//        pagination.find("a").on("click", function (e) {
//            e.preventDefault();
//            var page = parseInt($(this).text());
//            if ($(this).text() === "<") {
//                page = Math.max(1, currentPage - 1);
//            } else if ($(this).text() === ">") {
//                page = Math.min(totalPages, currentPage + 1);
//            }
//            showPage(page);
//        });
//    });
//});
