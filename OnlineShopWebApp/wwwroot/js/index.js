// Карта выбора точки доставки на странице оформления заказа
(function () {
    'use strict';

    function showMapFallback() {
        const box = document.getElementById('map');
        if (!box || box.dataset.fallback) return;
        box.dataset.fallback = '1';
        box.innerHTML = '<div class="h-100 d-grid text-center text-muted small p-4" style="place-items:center">' +
            '<div><i class="bi bi-map fs-2 d-block mb-2"></i>Карта сейчас недоступна — просто введите адрес вручную</div></div>';
    }

    function init() {
        if (!document.getElementById('map')) return;
        if (typeof ymaps === 'undefined') {
            showMapFallback();
            return;
        }

        ymaps.ready(function () {
            const map = new ymaps.Map('map', {
                center: [59.940063, 30.309840],
                zoom: 11,
                controls: ['zoomControl', 'geolocationControl']
            });

            const placemark = new ymaps.Placemark(map.getCenter(), {
                hintContent: 'Точка доставки'
            }, {
                preset: 'islands#darkGreenDotIcon',
                draggable: true
            });

            map.geoObjects.add(placemark);

            map.events.add('click', function (e) {
                const coords = e.get('coords');
                placemark.geometry.setCoordinates(coords);
                getAddressByCoords(coords);
            });

            placemark.events.add('dragend', function () {
                getAddressByCoords(placemark.geometry.getCoordinates());
            });
        });
    }

    function getAddressByCoords(coords) {
        ymaps.geocode(coords).then(
            function (res) {
                const first = res.geoObjects.get(0);
                updateAddressOnPage(first ? first.getAddressLine() : '');
            },
            function (err) {
                console.error('Ошибка геокодирования:', err);
            }
        );
    }

    function updateAddressOnPage(address) {
        const input = document.getElementById('deliveryAddress');
        if (!input || !address) return;
        input.value = address;
        input.dispatchEvent(new Event('change', { bubbles: true }));
        // Подсвечиваем поле, чтобы было видно, что адрес подставился
        input.animate([{ backgroundColor: '#e3f2e6' }, { backgroundColor: '#fff' }], { duration: 1200 });
        if (window.jQuery && jQuery.validator) jQuery(input).valid();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
    // Если скрипт API заблокирован (например, блокировщиком рекламы) — покажем подсказку
    window.addEventListener('load', function () {
        if (typeof ymaps === 'undefined') showMapFallback();
    });
})();
