// Load SweetAlert2 dynamically if not already in layout
const script = document.createElement('script');
script.src = "https://cdn.jsdelivr.net/npm/sweetalert2@11";
document.head.appendChild(script);

function initializeForm() {
    var form = document.getElementById('companyEditForm');
    var submitBtn = document.getElementById('submitBtn');

    if (form && submitBtn) {
        form.addEventListener('submit', function (e) {
            e.preventDefault();

            var name = document.getElementById('name').value.trim();
            var email = document.getElementById('email').value.trim();

            var isValid = true;
            var errors = [];

            if (!name) {
                errors.push('Şirket adı zorunludur');
                isValid = false;
            }

            if (!email) {
                errors.push('E-posta adresi zorunludur');
                isValid = false;
            } else if (!isValidEmail(email)) {
                errors.push('Geçerli bir e-posta adresi giriniz');
                isValid = false;
            }

            if (!isValid) {
                Swal.fire({
                    icon: 'error',
                    title: 'Form Hatası',
                    html: errors.join('<br>'),
                    confirmButtonText: 'Tamam'
                });
                return;
            }

            Swal.fire({
                title: 'Değişiklikleri Kaydet?',
                text: 'Firma bilgileri güncellenecek.',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#fd7e14',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Evet, Kaydet',
                cancelButtonText: 'İptal'
            }).then(function (result) {
                if (result.isConfirmed) {
                    submitBtn.innerHTML = 'Kaydediliyor...';
                    submitBtn.disabled = true;
                    form.submit();
                }
            });
        });
    }

    // Logo preview update
    var logoUrlInput = document.getElementById('logoUrl');
    if (logoUrlInput) {
        logoUrlInput.addEventListener('input', function () {
            var logoUrl = this.value;
            var preview = document.getElementById('logoPreview');

            if (preview && logoUrl && isValidUrl(logoUrl)) {
                preview.innerHTML = '<img src="' + logoUrl + '" alt="Logo" class="img-fluid" style="max-height: 100px;" onerror="showDefaultLogo()">';
            } else if (preview && !logoUrl) {
                showDefaultLogo();
            }
        });
    }
}

function resetForm() {
    var form = document.getElementById('companyEditForm');
    if (form) {
        Swal.fire({
            title: 'Formu Sıfırla?',
            text: 'Tüm değişiklikler kaybolacak.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#fd7e14',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Evet, Sıfırla',
            cancelButtonText: 'İptal'
        }).then(function (result) {
            if (result.isConfirmed) {
                form.reset();
                Swal.fire({
                    icon: 'success',
                    title: 'Form Sıfırlandı',
                    timer: 2000,
                    showConfirmButton: false,
                    toast: true,
                    position: 'top-end'
                });
            }
        });
    }
}

function isValidEmail(email) {
    var emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

function isValidUrl(string) {
    try {
        new URL(string);
        return true;
    } catch (_) {
        return false;
    }
}

function showDefaultLogo() {
    var preview = document.getElementById('logoPreview');
    if (preview) {
        preview.innerHTML = '<i class="fas fa-building fa-4x text-muted"></i><p class="text-muted mt-2">Logo URL girildiğinde burada görünecek</p>';
    }
}

document.addEventListener('DOMContentLoaded', initializeForm);
