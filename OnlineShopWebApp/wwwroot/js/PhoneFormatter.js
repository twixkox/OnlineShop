// phoneFormatter.js

/**
 * Форматирует ввод телефона в формате +7 (999) 999-99-99
 * @param {HTMLInputElement} input - поле ввода
 */
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

/**
 * Инициализирует форматирование для всех полей с классом 'phone-input'
 * (можно также передать другой селектор)
 */
function initPhoneFormatting(selector = '.phone-input') {
    document.querySelectorAll(selector).forEach(input => {
        // Форматируем текущее значение
        formatPhone(input);
        // Добавляем обработчик
        input.addEventListener('input', function () {
            formatPhone(this);
        });
    });
}

// Автоматически запускаем после полной загрузки DOM
document.addEventListener('DOMContentLoaded', () => initPhoneFormatting());