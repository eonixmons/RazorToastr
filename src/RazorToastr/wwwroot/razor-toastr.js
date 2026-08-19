/*!
 * RazorToastr — reads the toasts queued server-side and hands them to toastr.
 *
 * Deliberately dependency-free and framework-free so it can be served under a strict
 * Content-Security-Policy: this file is the only executable part of the package, and the
 * markup it reads carries no code at all.
 *
 * Contract with ToastrMessagesTagHelper:
 *   <div id="razor-toastr" data-razor-toastr='[{"level":"success","message":"…"}]' hidden></div>
 *
 * MIT — Copyright (c) 2026 eonixmons
 */
(function () {
    'use strict';

    var ELEMENT_ID = 'razor-toastr';
    var DATA_ATTRIBUTE = 'data-razor-toastr';
    var LEVELS = ['success', 'info', 'warning', 'error'];

    function show(toastr, entry) {
        if (!entry || typeof entry.message !== 'string' || entry.message === '') {
            return;
        }

        // Only ever dispatch to a known toastr function: an unexpected level in the payload
        // must not turn into an arbitrary property lookup on the toastr object.
        var level = LEVELS.indexOf(entry.level) === -1 ? 'info' : entry.level;

        // toastr treats both arguments as text, so a message is never interpreted as markup.
        if (typeof entry.title === 'string' && entry.title !== '') {
            toastr[level](entry.message, entry.title);
        } else {
            toastr[level](entry.message);
        }
    }

    function render() {
        var host = document.getElementById(ELEMENT_ID);
        if (!host) {
            return;
        }

        var payload = host.getAttribute(DATA_ATTRIBUTE);

        // Clear the attribute first: should anything below throw, a re-entrant call cannot
        // replay the same toasts.
        host.removeAttribute(DATA_ATTRIBUTE);

        if (!payload) {
            return;
        }

        var toastr = window.toastr;
        if (!toastr) {
            // toastr is supplied by the host application. Say so once, plainly, instead of
            // failing silently on a page the developer believes is wired up.
            if (window.console && window.console.warn) {
                window.console.warn('RazorToastr: toastr is not loaded, ' +
                    'so queued messages cannot be shown. Load toastr before razor-toastr.js.');
            }
            return;
        }

        var entries;
        try {
            entries = JSON.parse(payload);
        } catch (error) {
            return;
        }

        if (!Array.isArray(entries)) {
            return;
        }

        for (var i = 0; i < entries.length; i++) {
            show(toastr, entries[i]);
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', render);
    } else {
        render();
    }
})();
