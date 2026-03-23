function formatPhone(input) {
    let numbers = input.value.replace(/\D/g, '');
    if (numbers.startsWith('7') || numbers.startsWith('8')) {
        numbers = numbers.substring(1);
    }
    numbers = numbers.slice(0, 10);
    let formatted = '';
    if (numbers.length > 0) {
        formatted = '+7';
        if (numbers.length > 0) {
            formatted += ' (' + numbers.substring(0, 3);
        }
        if (numbers.length >= 4) {
            formatted += ') ' + numbers.substring(3, 6);
        }
        if (numbers.length >= 7) {
            formatted += '-' + numbers.substring(6, 8);
        }
        if (numbers.length >= 9) {
            formatted += '-' + numbers.substring(8, 10);
        }
    }
    input.value = formatted;
}

function initPhoneFormatting(selector = '.phone-input') {
    document.querySelectorAll(selector).forEach(input => {
        formatPhone(input);
        input.addEventListener('input', function () {
            formatPhone(this);
        });
    });
}

document.addEventListener('DOMContentLoaded', () => initPhoneFormatting());