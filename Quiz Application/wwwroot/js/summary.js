// ===== summary.js — Summary Page =====
(function () {
    'use strict';

    $(document).ready(function () {
        const params = new URLSearchParams(window.location.search);
        const sessionId = params.get('sessionId');

        if (!sessionId) {
            $('#summary-spinner').hide();
            $('#summary-container').show().html(
                '<div class="validation-error">No session found. <a href="/Home/Quiz" style="color:var(--primary-light)">Start a quiz</a>.</div>'
            );
            return;
        }

        loadSummary(sessionId);
    });

    async function loadSummary(sessionId) {
        const res = await QuizApi.fetchSummary(sessionId);

        $('#summary-spinner').hide();

        if (!res.ok) {
            $('#summary-container').show().html(
                `<div class="validation-error">Failed to load summary: ${res.error}</div>`
            );
            return;
        }

        const { questionSummaries, stats } = res.data;
        renderStats(stats);
        renderTable(questionSummaries);
        $('#summary-container').show();
    }

    function renderStats(s) {
        const cards = [
            { label: 'Total', value: s.total, cls: 'total' },
            { label: 'Attempted', value: s.attempted, cls: 'attempted' },
            { label: 'Correct', value: s.correct, cls: 'correct' },
            { label: 'Score', value: s.percentage + '%', cls: 'score' }
        ];

        const $row = $('#stats-row').empty();
        cards.forEach((c, i) => {
            $row.append(`
                <div class="col-md-3 col-6 mb-3">
                    <div class="stat-card ${c.cls}" style="animation-delay: ${i * 0.1}s">
                        <div class="stat-label">${c.label}</div>
                        <div class="stat-value">${c.value}</div>
                    </div>
                </div>
            `);
        });
    }

    function getBadgeClass(type) {
        const map = {
            'SingleChoice': 'single',
            'MultipleChoice': 'multiple',
            'ShortAnswer': 'short',
            'PhoneNumber': 'phone',
            'LongAnswer': 'long'
        };
        return map[type] || '';
    }

    function renderTable(items) {
        const $body = $('#summary-body').empty();
        items.forEach((item, i) => {
            let resultBadge;
            if (item.isCorrect === true)
                resultBadge = '<span class="result-badge correct">✓ Correct</span>';
            else if (item.isCorrect === false)
                resultBadge = '<span class="result-badge wrong">✗ Wrong</span>';
            else
                resultBadge = '<span class="result-badge na">— N/A</span>';

            $body.append(`<tr>
                <td style="font-weight:600; color:var(--text-primary)">${i + 1}</td>
                <td style="color:var(--text-primary)">${item.questionText}</td>
                <td><span class="badge-type ${getBadgeClass(item.questionType)}">${item.questionType}</span></td>
                <td>${item.userAnswer || '<span style="color:var(--text-muted)">—</span>'}</td>
                <td>${item.correctAnswer || '<span style="color:var(--text-muted)">—</span>'}</td>
                <td>${resultBadge}</td>
            </tr>`);
        });
    }
})();
