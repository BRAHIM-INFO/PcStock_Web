$(document).ready(function () {
    // 1. معاينة الصورة عند اختيارها
    $('#logoUpload').change(function (e) {
        var reader = new FileReader();
        reader.onload = function (e) {
            $('#imgPreview').attr('src', e.target.result);
        }
        reader.readAsDataURL(this.files[0]);
    });

    // 2. محاكاة اختيار المسار (Browse)
    // كود زر Parcourir
    $('#btnBrowse').click(function (e) {
        e.preventDefault(); // لمنع أي سلوك افتراضي

        // جرب إظهار رسالة بسيطة للتأكد من أنه يعمل
        Swal.fire({
            title: 'Information',
            text: 'Veuillez saisir le chemin manuellement dans le champ texte.',
            icon: 'info'
        });
    });

    //$('#btnBrowse').click(function () {
    //    let currentPath = $('#dbPath').val() || "";

    //    // سنستخدم نافذة prompt بسيطة حالياً أو يمكنك بناء Modal
    //    let newPath = prompt("Entrez le chemin manuellement أو تأكد من المسار على السيرفر:", currentPath);

    //    if (newPath != null) {
    //        $('#dbPath').val(newPath);
    //    }
    //});

    // 3. اختبار الاتصال بقاعدة بيانات DBF
    $('#btnTestConnection').click(function () {
        const path = $('#dbPath').val();
        $(this).html('<span class="spinner-border spinner-border-sm"></span> Verifying...');

        $.ajax({
            // التعديل 1: الرابط يجب أن يشير للـ Handler في Razor Pages
            url: '?handler=TestConnection',
            type: 'POST',
            // التعديل 2: إضافة رمز الحماية (ضروري جداً في Razor Pages)
            headers: {
                "RequestVerificationToken": $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            data: { path: path },
            success: function (response) {
                if (response.success) {
                    //    alert(response.message); // سيعرض رسالة النجاح بالفرنسية
                    // رسالة النجاح بتصميم احترافي
                    Swal.fire({
                        title: 'Succès !',
                        text: response.message,
                        icon: 'success',
                        confirmButtonColor: '#2563eb', // نفس لون أزرار تطبيقك
                        confirmButtonText: 'OK'
                    });
                } else {
                    // رسالة الخطأ بتصميم احترافي
                    Swal.fire({
                        title: 'Erreur',
                        text: response.message,
                        icon: 'error',
                        confirmButtonColor: '#d33',
                        confirmButtonText: 'Fermer'
                    });
                //    alert("Erreur: " + response.message);
                }
            },
            error: function () {
                Swal.fire({
                    title: 'Erreur Système',
                    text: 'Impossible de contacter le serveur.',
                    icon: 'warning'
                });
            //    alert("Erreur نظام: تأكد من تشغيل المشروع بشكل صحيح.");
            },
            complete: function () {
                $('#btnTestConnection').html('<i class="fas fa-vial me-1"></i> Test Connection');
            }
        });
    });

    //@if (TempData["SuccessMessage"] != null) {
    //    <script>
    //        Swal.fire({
    //            title: 'Enregistré !',
    //        text: '@TempData["SuccessMessage"]',
    //        icon: 'success',
    //        confirmButtonColor: '#2563eb'
    //    });
    //    </script>
    //}

});