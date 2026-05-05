/**
 * UserTracker - Client Fingerprint Collector
 * Coleta o máximo de informações do dispositivo via browser APIs
 * e envia para o servidor via POST /api/tracking/collect
 */
(function () {
    'use strict';

    // ─── Utilitários ────────────────────────────────────────────────────────────

    async function sha256(str) {
        const buf = new TextEncoder().encode(str);
        const hash = await crypto.subtle.digest('SHA-256', buf);
        return Array.from(new Uint8Array(hash))
            .map(b => b.toString(16).padStart(2, '0'))
            .join('');
    }

    function safe(fn) {
        try { return fn(); } catch (_) { return null; }
    }

    async function safeAsync(fn) {
        try { return await fn(); } catch (_) { return null; }
    }

    // ─── Canvas Fingerprint ──────────────────────────────────────────────────────

    function getCanvasHash() {
        return safe(() => {
            const canvas = document.createElement('canvas');
            canvas.width = 240;
            canvas.height = 80;
            const ctx = canvas.getContext('2d');
            // Texto com fontes e cores específicas
            ctx.textBaseline = 'top';
            ctx.font = '16px Arial';
            ctx.fillStyle = '#f60';
            ctx.fillRect(0, 0, 10, 10);
            ctx.fillStyle = '#069';
            ctx.fillText('UserTracker 🔍', 2, 15);
            ctx.fillStyle = 'rgba(102,204,0,0.7)';
            ctx.fillText('Fingerprint', 4, 40);
            // Arco
            ctx.beginPath();
            ctx.arc(60, 60, 15, 0, Math.PI * 2, true);
            ctx.closePath();
            ctx.fillStyle = 'rgb(255, 0, 255)';
            ctx.fill();
            return canvas.toDataURL().slice(-80); // últimos 80 chars como hash
        });
    }

    // ─── WebGL Info ──────────────────────────────────────────────────────────────

    function getWebGlInfo() {
        return safe(() => {
            const canvas = document.createElement('canvas');
            const gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');
            if (!gl) return { vendor: null, renderer: null, version: null };
            const dbg = gl.getExtension('WEBGL_debug_renderer_info');
            return {
                vendor:   dbg ? gl.getParameter(dbg.UNMASKED_VENDOR_WEBGL)   : gl.getParameter(gl.VENDOR),
                renderer: dbg ? gl.getParameter(dbg.UNMASKED_RENDERER_WEBGL) : gl.getParameter(gl.RENDERER),
                version:  gl.getParameter(gl.VERSION)
            };
        }) ?? { vendor: null, renderer: null, version: null };
    }

    // ─── Storage Checks ───────────────────────────────────────────────────────────

    function checkStorage(type) {
        return safe(() => {
            const s = window[type];
            const key = '__ut_test__';
            s.setItem(key, '1');
            s.removeItem(key);
            return true;
        }) ?? false;
    }

    function checkIndexedDb() {
        return safe(() => !!window.indexedDB) ?? false;
    }

    function checkWebRtc() {
        return safe(() =>
            !!(window.RTCPeerConnection ||
               window.webkitRTCPeerConnection ||
               window.mozRTCPeerConnection)
        ) ?? false;
    }

    // ─── Network Info ─────────────────────────────────────────────────────────────

    function getConnectionInfo() {
        const conn = navigator.connection
            || navigator.mozConnection
            || navigator.webkitConnection;
        if (!conn) return { type: null, downlink: null };
        return {
            type:     conn.effectiveType ?? null,
            downlink: conn.downlink ?? null
        };
    }

    // ─── Coleta principal ─────────────────────────────────────────────────────────

    async function collect() {
        const nav = navigator;
        const scr = screen;
        const gl  = getWebGlInfo();
        const net = getConnectionInfo();
        const canvas = getCanvasHash();

        const plugins = safe(() =>
            Array.from(nav.plugins || []).map(p => p.name).join(', ')
        ) ?? '';

        // Monta string única para hash
        const fingerprintSource = [
            nav.userAgent,
            nav.language,
            Intl.DateTimeFormat().resolvedOptions().timeZone,
            scr.width, scr.height, scr.colorDepth,
            nav.hardwareConcurrency,
            nav.deviceMemory,
            gl.renderer,
            canvas,
            plugins
        ].join('|');

        const fingerprintHash = await sha256(fingerprintSource);

        const payload = {
            fingerprintHash,

            // Navegador
            language:    nav.language ?? null,
            languages:   (nav.languages ?? []).join(', '),
            platform:    nav.platform ?? null,
            timezone:    safe(() => Intl.DateTimeFormat().resolvedOptions().timeZone),
            timezoneOffset: new Date().getTimezoneOffset(),
            cookiesEnabled: nav.cookieEnabled ?? null,
            plugins,

            // Tela
            screenWidth:  scr.width,
            screenHeight: scr.height,
            availWidth:   scr.availWidth,
            availHeight:  scr.availHeight,
            colorDepth:   scr.colorDepth,
            pixelRatio:   window.devicePixelRatio ?? null,

            // Hardware
            hardwareConcurrency: nav.hardwareConcurrency ?? null,
            deviceMemory:        nav.deviceMemory ?? null,
            maxTouchPoints:      nav.maxTouchPoints ?? null,

            // Capacidades
            localStorage:   checkStorage('localStorage'),
            sessionStorage: checkStorage('sessionStorage'),
            indexedDb:      checkIndexedDb(),
            webRtc:         checkWebRtc(),

            // Rede
            connectionType:     net.type,
            connectionDownlink: net.downlink,

            // Canvas & WebGL
            canvasHash:  canvas,
            webGlVendor:   gl.vendor,
            webGlRenderer: gl.renderer,
            webGlVersion:  gl.version
        };

        // Envia ao servidor
        try {
            const res = await fetch('/api/tracking/collect', {
                method:  'POST',
                headers: { 'Content-Type': 'application/json' },
                body:    JSON.stringify(payload)
            });
            if (res.ok) {
                const data = await res.json();
                console.debug('[UserTracker] Acesso registrado. ID:', data.id, '| Hash:', data.fingerprintHash?.slice(0, 16) + '...');
                // Armazena o hash localmente para uso na sessão
                try { sessionStorage.setItem('ut_fingerprint', data.fingerprintHash); } catch (_) {}
            }
        } catch (err) {
            console.warn('[UserTracker] Falha ao enviar fingerprint:', err.message);
        }
    }

    // Aguarda DOM pronto e dispara
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', collect);
    } else {
        collect();
    }
})();
