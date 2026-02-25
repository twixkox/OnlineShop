
let map;          // объект карты
let placemark;    // метка, которая будет стоять в центре

// ===== 1. ИНИЦИАЛИЗАЦИЯ КАРТЫ (ждём загрузки API) =====
ymaps.ready(initMap);

function initMap() {
    // Создаём карту. Первый параметр – id контейнера, второй – настройки.
    map = new ymaps.Map('map', {
        center: [59.940063, 30.309840], // ВНИМАНИЕ: в 2.1 порядок [широта, долгота]!
        zoom: 16,
        // Добавляем стандартные элементы управления: зум и полноэкранный режим
        controls: ['zoomControl', 'fullscreenControl']
    });

    // Создаём метку в центре карты
    // В 2.1 метка называется Placemark
    placemark = new ymaps.Placemark(map.getCenter(), {
        // Свойства метки (можно добавить подсказку)
        hintContent: 'Точка доставки'
    }, {
        // Опции внешнего вида
        iconLayout: 'default#image',
        // Используем иконку Bootstrap Icons (можно заменить на свою картинку)
        iconImageHref: 'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/icons/geo-alt-fill.svg',
        iconImageSize: [40, 40],
        // Смещение, чтобы остриё иконки указывало точно на координаты
        iconImageOffset: [-20, -40]
    });

    // Добавляем метку на карту
    map.geoObjects.add(placemark);

    // Подписываемся на событие клика по карте
    map.events.add('click', onMapClick);
}

function onMapClick(e) {
    // Получаем координаты клика. В 2.1 они приходят в виде массива [широта, долгота].
    const coords = e.get('coords');

    placemark.geometry.setCoordinates(coords);

    getAddressByCoords(coords);
}

function getAddressByCoords(coords) {

    ymaps.geocode(coords).then(
        function (res) {

            const firstGeoObject = res.geoObjects.get(0);
            if (firstGeoObject) {
  
                const address = firstGeoObject.getAddressLine();

                updateAddressOnPage(address);
            } else {
                updateAddressOnPage('Адрес не найден');
            }
        },
        function (err) {
            console.error('Ошибка геокодирования:', err);
            updateAddressOnPage('Ошибка определения адреса');
        }
    );
}


function updateAddressOnPage(address) {
    const addressElement = document.getElementById('deliveryAddress');
    if (addressElement) {
        addressElement.value = address;
    }
}