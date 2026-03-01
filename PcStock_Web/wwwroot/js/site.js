// تعليق: هذا الكود يقوم بتبديل حالة القائمة الجانبية
document.addEventListener("DOMContentLoaded", function () {
    const sidebar = document.getElementById('sidebar');
    const toggleBtn = document.getElementById('sidebarToggle');
    const icon = document.getElementById('toggleIcon');

    toggleBtn.addEventListener('click', function () {
        // تبديل الكلاس collapsed
        sidebar.classList.toggle('collapsed');

        // تغيير شكل الأيقونة عند الفتح والإغلاق
        if (sidebar.classList.contains('collapsed')) {
            icon.classList.replace('fa-bars', 'fa-chevron-right');
        } else {
            icon.classList.replace('fa-chevron-right', 'fa-bars');
        }
    });
});

// تعليق: إضافة كلاس active للعنصر المختار وإزالته من الآخرين
$('.submenu-item').on('click', function () {
    $('.submenu-item').removeClass('active');
    $(this).addClass('active');
});