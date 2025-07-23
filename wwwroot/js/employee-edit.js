// Load SweetAlert if not already in the layout
const script = document.createElement('script');
script.src = "https://cdn.jsdelivr.net/npm/sweetalert2@11";
document.head.appendChild(script);

// Form validation and interaction
document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('employeeEditForm');
    const submitBtn = document.getElementById('submitBtn');

    if (form && submitBtn) {
        // Form validation
        form.addEventListener('submit', function (e) {
            e.preventDefault();

            const firstName = document.getElementById('firstName').value.trim();
            const lastName = document.getElementById('lastName').value.trim();
            const companyId = document.getElementById('companyId').value;

            let isValid = true;
            let errors = [];

            if (!firstName) {
                errors.push('Ad alanı zorunludur');
                isValid = false;
            }

            if (!lastName) {
                errors.push('Soyad alanı zorunludur');
                isValid = false;
            }

            if (!companyId) {
                errors.push('Firma seçimi zorunludur');
                isValid = false;
            }

            const emailElement = document.getElementById('email');
            if (emailElement) {
                const email = emailElement.value.trim();
                if (email && !isValidEmail(email)) {
                    errors.push('Geçerli bir e-posta adresi giriniz');
                    isValid = false;
                }
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
                text: 'Çalışan bilgileri güncellenecek. Devam etmek istediğinize emin misiniz?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#0d6efd',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Evet, Kaydet',
                cancelButtonText: 'İptal'
            }).then((result) => {
                if (result.isConfirmed) {
                    submitBtn.innerHTML = 'Kaydediliyor...';
                    submitBtn.disabled = true;
                    form.submit();
                }
            });
        });

        const inputs = form.querySelectorAll('input, select');
        inputs.forEach(input => {
            input.addEventListener('blur', function () {
                validateField(this);
            });
        });
    }
});

// Reset form function
function resetForm() {
    const form = document.getElementById('employeeEditForm');
    if (form) {
        Swal.fire({
            title: 'Formu Sıfırla?',
            text: 'Tüm değişiklikler kaybolacak. Devam etmek istediğinize emin misiniz?',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#fd7e14',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Evet, Sıfırla',
            cancelButtonText: 'İptal'
        }).then((result) => {
            if (result.isConfirmed) {
                form.reset();

                const invalidElements = document.querySelectorAll('.is-invalid');
                invalidElements.forEach(el => {
                    el.classList.remove('is-invalid');
                });

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

// Field validation function
function validateField(field) {
    if (!field) return;

    const value = field.value.trim();
    let isValid = true;

    field.classList.remove('is-valid', 'is-invalid');

    if (field.hasAttribute('required') && !value) {
        isValid = false;
    } else if (field.type === 'email' && value && !isValidEmail(value)) {
        isValid = false;
    }

    field.classList.add(isValid ? 'is-valid' : 'is-invalid');
}

// Email validation helper
function isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}
