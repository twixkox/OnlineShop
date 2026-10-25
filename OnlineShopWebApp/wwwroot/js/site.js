/* =========================================================
   PlantShop — интерактив и анимации
   ========================================================= */
(function () {
    'use strict';

    document.documentElement.classList.remove('no-js');

    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const $ = (sel, root = document) => root.querySelector(sel);
    const $$ = (sel, root = document) => Array.from(root.querySelectorAll(sel));

    /* ---------- Уведомления ---------- */
    const icons = { success: 'bi-check-lg', info: 'bi-info-lg', error: 'bi-exclamation-lg' };

    function toast(message, type = 'success', timeout = 3800) {
        if (!message) return;
        let stack = $('.toast-stack');
        if (!stack) {
            stack = document.createElement('div');
            stack.className = 'toast-stack';
            stack.setAttribute('aria-live', 'polite');
            document.body.appendChild(stack);
        }

        const el = document.createElement('div');
        el.className = `ps-toast ${type}`;
        el.setAttribute('role', 'status');
        el.style.setProperty('--t', `${timeout}ms`);
        el.innerHTML = `
            <span class="t-ico"><i class="bi ${icons[type] || icons.info}"></i></span>
            <span class="t-text"></span>
            <button type="button" class="t-close" aria-label="Закрыть"><i class="bi bi-x-lg"></i></button>
            <span class="t-progress"></span>`;
        el.querySelector('.t-text').textContent = message;
        stack.appendChild(el);

        const close = () => {
            el.classList.add('is-leaving');
            el.addEventListener('animationend', () => el.remove(), { once: true });
        };
        el.querySelector('.t-close').addEventListener('click', close);
        setTimeout(close, timeout);
    }

    window.PlantShop = { toast };

    // Сообщения от сервера (TempData) приходят через data-атрибуты
    $$('[data-flash]').forEach(node => toast(node.dataset.flash, node.dataset.type || 'success'));

    /* ---------- Шапка, прогресс прокрутки, «наверх» ---------- */
    const header = $('.site-header');
    const progress = $('.scroll-progress');
    const toTop = $('.to-top');
    let ticking = false;

    function onScroll() {
        const y = window.scrollY;
        header?.classList.toggle('is-scrolled', y > 10);
        toTop?.classList.toggle('is-visible', y > 600);
        if (progress) {
            const max = document.documentElement.scrollHeight - window.innerHeight;
            progress.style.transform = `scaleX(${max > 0 ? y / max : 0})`;
        }
        ticking = false;
    }

    window.addEventListener('scroll', () => {
        if (!ticking) {
            requestAnimationFrame(onScroll);
            ticking = true;
        }
    }, { passive: true });
    onScroll();

    toTop?.addEventListener('click', () => window.scrollTo({ top: 0, behavior: reduceMotion ? 'auto' : 'smooth' }));

    /* ---------- Появление при прокрутке ---------- */
    function observeReveal(root = document) {
        const items = $$('[data-reveal]:not(.is-visible)', root);
        if (!('IntersectionObserver' in window) || reduceMotion) {
            items.forEach(i => i.classList.add('is-visible'));
            return;
        }
        const io = new IntersectionObserver(entries => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-visible');
                    io.unobserve(entry.target);
                }
            });
        }, { rootMargin: '0px 0px -8% 0px', threshold: .08 });
        items.forEach(i => io.observe(i));
    }

    // Автоматическая «лесенка» задержек для элементов сетки
    $$('[data-stagger]').forEach(group => {
        const step = parseFloat(group.dataset.stagger) || 0.07;
        Array.from(group.children).forEach((child, idx) => {
            child.setAttribute('data-reveal', '');
            child.style.setProperty('--d', `${Math.min(idx, 8) * step}s`);
        });
    });
    observeReveal();

    /* ---------- Эффект «волны» на кнопках ---------- */
    document.addEventListener('pointerdown', e => {
        const btn = e.target.closest('.btn, .cart-btn');
        if (!btn || reduceMotion) return;
        const rect = btn.getBoundingClientRect();
        const ripple = document.createElement('span');
        ripple.className = 'ripple';
        ripple.style.left = `${e.clientX - rect.left}px`;
        ripple.style.top = `${e.clientY - rect.top}px`;
        btn.appendChild(ripple);
        ripple.addEventListener('animationend', () => ripple.remove());
    });

    /* ---------- Счётчики в шапке ---------- */
    function setCounter(name, value) {
        $$(`[data-counter="${name}"]`).forEach(c => {
            c.textContent = value > 0 ? value : '';
            c.classList.remove('bump');
            void c.offsetWidth; // перезапуск анимации
            c.classList.add('bump');
        });
    }

    /* ---------- «Полёт» товара в корзину ---------- */
    function flyToCart(sourceImg) {
        const target = $('[data-cart-target]');
        if (!sourceImg || !target || reduceMotion) return;
        const from = sourceImg.getBoundingClientRect();
        const to = target.getBoundingClientRect();
        if (!from.width) return;

        const clone = sourceImg.cloneNode();
        clone.className = 'fly-item';
        Object.assign(clone.style, {
            left: `${from.left}px`, top: `${from.top}px`,
            width: `${from.width}px`, height: `${from.height}px`
        });
        document.body.appendChild(clone);

        requestAnimationFrame(() => {
            const dx = to.left + to.width / 2 - (from.left + from.width / 2);
            const dy = to.top + to.height / 2 - (from.top + from.height / 2);
            const scale = 28 / from.width;
            clone.style.transform = `translate(${dx}px, ${dy}px) scale(${scale})`;
            clone.style.opacity = '.4';
            clone.style.borderRadius = '50%';
        });

        clone.addEventListener('transitionend', () => {
            clone.remove();
            target.classList.add('shake');
            setTimeout(() => target.classList.remove('shake'), 800);
        }, { once: true });
    }

    async function ajax(url) {
        const response = await fetch(url, {
            headers: { 'X-Requested-With': 'XMLHttpRequest', 'Accept': 'application/json' },
            credentials: 'same-origin'
        });
        if (response.status === 401) {
            const err = new Error('unauthorized');
            err.unauthorized = true;
            throw err;
        }
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        return response.json();
    }

    function askToLogin() {
        const modalEl = $('#loginModal');
        if (modalEl && window.bootstrap) {
            bootstrap.Modal.getOrCreateInstance(modalEl).show();
            toast('Войдите, чтобы добавлять товары в корзину и избранное', 'info');
        } else {
            window.location.href = '/Authorization/Index?returnUrl=' + encodeURIComponent(location.pathname + location.search);
        }
    }

    /* ---------- В корзину ---------- */
    document.addEventListener('click', async e => {
        const btn = e.target.closest('[data-add-to-cart]');
        if (!btn) return;
        e.preventDefault();
        if (btn.classList.contains('is-loading')) return;

        btn.classList.add('is-loading');
        try {
            const data = await ajax(btn.href || btn.dataset.url);
            const card = btn.closest('.product-card, [data-product]');
            flyToCart(card?.querySelector('img'));
            setCounter('cart', data.count);
            toast(data.message || 'Товар добавлен в корзину');

            const icon = btn.querySelector('.bi');
            const label = btn.querySelector('.label');
            btn.classList.add('is-added');
            icon?.classList.replace('bi-bag-plus', 'bi-check-lg');
            const oldText = label?.textContent;
            if (label) label.textContent = 'В корзине';
            setTimeout(() => {
                btn.classList.remove('is-added');
                icon?.classList.replace('bi-check-lg', 'bi-bag-plus');
                if (label) label.textContent = oldText;
            }, 1800);
        } catch (err) {
            if (err.unauthorized) askToLogin();
            else toast('Не удалось добавить товар. Попробуйте ещё раз', 'error');
        } finally {
            btn.classList.remove('is-loading');
        }
    });

    /* ---------- Избранное ---------- */
    document.addEventListener('click', async e => {
        const btn = e.target.closest('[data-fav-toggle]');
        if (!btn) return;
        e.preventDefault();

        try {
            const data = await ajax(btn.href || btn.dataset.url);
            $$(`[data-fav-toggle][data-id="${btn.dataset.id}"]`).forEach(b => {
                b.classList.toggle('is-active', data.inFavorites);
                const i = b.querySelector('.bi');
                i?.classList.toggle('bi-heart-fill', data.inFavorites);
                i?.classList.toggle('bi-heart', !data.inFavorites);
                b.setAttribute('aria-pressed', data.inFavorites);
                const label = b.querySelector('.label');
                if (label) label.textContent = data.inFavorites ? 'В избранном' : 'В избранное';
                b.classList.remove('pop');
                void b.offsetWidth;
                b.classList.add('pop');
            });
            setCounter('fav', data.count);
            toast(data.message, data.inFavorites ? 'success' : 'info', 2500);

            // На странице избранного карточка плавно исчезает
            const removable = btn.closest('[data-remove-on-unfav]');
            if (removable && !data.inFavorites) {
                removable.classList.add('is-hiding');
                setTimeout(() => {
                    removable.remove();
                    if (!$('[data-remove-on-unfav]')) location.reload();
                }, 350);
            }
        } catch (err) {
            if (err.unauthorized) askToLogin();
            else toast('Не удалось обновить избранное', 'error');
        }
    });

    /* ---------- Корзина: количество и удаление без перезагрузки ---------- */
    document.addEventListener('click', async e => {
        const btn = e.target.closest('[data-cart-action]');
        if (!btn) return;
        e.preventDefault();

        const row = btn.closest('.cart-row');
        try {
            const data = await ajax(btn.href);
            setCounter('cart', data.count);

            if (data.count === 0) {
                location.reload();
                return;
            }

            if (row) {
                if (data.itemQuantity === 0) {
                    row.classList.add('is-removing');
                    setTimeout(() => row.remove(), 350);
                } else {
                    const q = row.querySelector('[data-qty]');
                    const c = row.querySelector('[data-line-total]');
                    if (q) q.textContent = data.itemQuantity;
                    if (c) c.textContent = data.itemCost;
                }
            }
            $$('[data-cart-total]').forEach(n => n.textContent = data.total);
            $$('[data-cart-count]').forEach(n => n.textContent = data.count);
            updateFreeShipping(data.total);
            if (data.itemQuantity === 0) toast('Товар удалён из корзины', 'info', 2500);
        } catch {
            window.location.href = btn.href;
        }
    });

    function updateFreeShipping(totalText) {
        const box = $('[data-free-ship]');
        if (!box) return;
        const limit = parseFloat(box.dataset.freeShip);
        const total = parseFloat(String(totalText).replace(/[^\d,]/g, '').replace(',', '.')) || 0;
        const left = Math.max(0, limit - total);
        const bar = box.querySelector('.progress-bar');
        if (bar) bar.style.width = `${Math.min(100, total / limit * 100)}%`;
        const text = box.querySelector('[data-free-ship-text]');
        if (text) {
            text.innerHTML = left > 0
                ? `До бесплатной доставки осталось <strong>${left.toLocaleString('ru-RU')} ₽</strong>`
                : '<strong>Доставка будет бесплатной</strong> 🎉';
        }
    }

    /* ---------- Каталог: сортировка и мгновенный поиск ---------- */
    const grid = $('[data-catalog-grid]');
    if (grid) {
        const items = $$('.product-item', grid);
        items.forEach((item, idx) => item.dataset.index = idx);
        const search = $('[data-catalog-search]');
        const counter = $('[data-catalog-count]');
        const empty = $('[data-catalog-empty]');
        let currentSort = 'default';

        function apply() {
            const q = (search?.value || '').trim().toLowerCase();
            let visible = 0;

            const sorted = [...items].sort((a, b) => {
                const pa = parseFloat(a.dataset.price), pb = parseFloat(b.dataset.price);
                switch (currentSort) {
                    case 'price-asc': return pa - pb;
                    case 'price-desc': return pb - pa;
                    case 'name': return a.dataset.name.localeCompare(b.dataset.name, 'ru');
                    default: return a.dataset.index - b.dataset.index;
                }
            });

            // FLIP-анимация: карточки плавно «переезжают» на новые места
            const first = new Map(items.map(i => [i, i.getBoundingClientRect()]));
            sorted.forEach(i => grid.appendChild(i));

            sorted.forEach(item => {
                const match = !q || item.dataset.name.includes(q);
                item.hidden = !match;
                if (match) visible++;
            });

            if (!reduceMotion) {
                sorted.forEach(item => {
                    if (item.hidden) return;
                    const a = first.get(item);
                    const b = item.getBoundingClientRect();
                    const dx = a.left - b.left, dy = a.top - b.top;
                    if (!a.width || (!dx && !dy)) return;
                    item.animate(
                        [{ transform: `translate(${dx}px, ${dy}px)` }, { transform: 'none' }],
                        { duration: 500, easing: 'cubic-bezier(.22,1,.36,1)' }
                    );
                });
            }

            if (counter) counter.textContent = visible;
            if (empty) empty.hidden = visible !== 0;
        }

        $$('[data-sort]').forEach(btn => btn.addEventListener('click', () => {
            $$('[data-sort]').forEach(b => b.classList.toggle('active', b === btn));
            currentSort = btn.dataset.sort;
            apply();
        }));

        let t;
        search?.addEventListener('input', () => {
            clearTimeout(t);
            t = setTimeout(apply, 120);
        });
    }

    /* ---------- Показать/скрыть пароль ---------- */
    window.togglePassword = function (inputId, el) {
        const input = document.getElementById(inputId);
        const icon = el.querySelector('i');
        if (!input) return;
        const show = input.type === 'password';
        input.type = show ? 'text' : 'password';
        icon?.classList.toggle('bi-eye', !show);
        icon?.classList.toggle('bi-eye-slash', show);
    };

    /* ---------- Кнопки отправки форм: индикатор загрузки ---------- */
    document.addEventListener('submit', e => {
        const form = e.target;
        if (form.matches('[data-no-loader]')) return;
        // jquery-validation отменяет отправку невалидной формы — тогда индикатор не нужен
        setTimeout(() => {
            if (e.defaultPrevented) return;
            const btn = form.querySelector('[type="submit"]');
            if (!btn || btn.classList.contains('is-loading')) return;
            btn.classList.add('is-loading');
            btn.dataset.html = btn.innerHTML;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>Отправка...';
        }, 0);
    });

    // Если пользователь вернулся «назад» — восстанавливаем кнопки
    window.addEventListener('pageshow', () => {
        $$('[type="submit"].is-loading').forEach(btn => {
            btn.classList.remove('is-loading');
            if (btn.dataset.html) btn.innerHTML = btn.dataset.html;
        });
    });

    /* ---------- Подтверждение действий ---------- */
    document.addEventListener('click', e => {
        const el = e.target.closest('[data-confirm]');
        if (el && !confirm(el.dataset.confirm)) e.preventDefault();
    });

    /* ---------- Увеличение фото товара под курсором ---------- */
    $$('.product-gallery').forEach(box => {
        const img = box.querySelector('img');
        box.addEventListener('mousemove', e => {
            const r = box.getBoundingClientRect();
            img.style.setProperty('--zx', `${((e.clientX - r.left) / r.width) * 100}%`);
            img.style.setProperty('--zy', `${((e.clientY - r.top) / r.height) * 100}%`);
        });
    });

    /* ---------- Картинки: заглушка, если файл не загрузился ---------- */
    document.addEventListener('error', e => {
        const img = e.target;
        if (img.tagName !== 'IMG' || img.dataset.fallbackApplied) return;
        img.dataset.fallbackApplied = '1';
        img.src = '/img/plant-placeholder.svg';
        img.classList.add('is-placeholder');
    }, true);

    /* ---------- Счётчики-«бегунки» на главной ---------- */
    const counters = $$('[data-count-to]');
    if (counters.length && 'IntersectionObserver' in window) {
        const io = new IntersectionObserver(entries => {
            entries.forEach(entry => {
                if (!entry.isIntersecting) return;
                const el = entry.target;
                const target = parseInt(el.dataset.countTo, 10);
                const suffix = el.dataset.suffix || '';
                const start = performance.now();
                const duration = reduceMotion ? 1 : 1600;
                (function frame(now) {
                    const p = Math.min(1, (now - start) / duration);
                    const eased = 1 - Math.pow(1 - p, 3);
                    el.textContent = Math.round(target * eased).toLocaleString('ru-RU') + suffix;
                    if (p < 1) requestAnimationFrame(frame);
                })(start);
                io.unobserve(el);
            });
        }, { threshold: .6 });
        counters.forEach(c => io.observe(c));
    }

    /* ---------- Конфетти (страница успешного заказа) ---------- */
    window.PlantShop.confetti = function () {
        if (reduceMotion) return;
        const colors = ['#40916c', '#74c69d', '#c8693f', '#ffd8a8', '#2d6a4f'];
        for (let i = 0; i < 90; i++) {
            const c = document.createElement('span');
            c.className = 'confetti';
            c.style.left = `${Math.random() * 100}vw`;
            c.style.background = colors[i % colors.length];
            document.body.appendChild(c);
            const fall = c.animate([
                { transform: `translate(0, 0) rotate(0deg)`, opacity: 1 },
                { transform: `translate(${(Math.random() - .5) * 240}px, ${window.innerHeight + 40}px) rotate(${Math.random() * 720}deg)`, opacity: .9 }
            ], { duration: 2200 + Math.random() * 1800, delay: Math.random() * 600, easing: 'cubic-bezier(.25,.6,.4,1)' });
            fall.onfinish = () => c.remove();
        }
    };
})();
