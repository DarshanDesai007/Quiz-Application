// ===== grid.js — Question Grid Page =====
(function () {
    'use strict';

    $(document).ready(function () {
        loadGrid();
    });

    async function loadGrid() {
        const res = await QuizApi.fetchAllQuestions();

        $('#grid-spinner').hide();

        if (!res.ok) {
            $('#grid-container').show().html(
                `<div class="validation-error">Failed to load questions: ${res.error}</div>`
            );
            return;
        }

        const $body = $('#grid-body').empty();
        res.data.forEach(q => $body.append(buildGridRow(q)));
        $('#grid-container').show();
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

    function buildGridRow(q) {
        let correctDisplay;
        switch (q.questionType) {
            case 'MultipleChoice':
                correctDisplay = '<span style="color:var(--text-muted)">Multiple</span>';
                break;
            case 'ShortAnswer':
            case 'LongAnswer':
            case 'PhoneNumber':
                correctDisplay = '<span style="color:var(--text-muted)">N/A</span>';
                break;
            default:
                correctDisplay = q.correctAnswer || '—';
        }

        return `<tr>
            <td style="font-weight:600; color:var(--text-primary)">${q.orderNo}</td>
            <td style="color:var(--text-primary)">${q.questionText}</td>
            <td><span class="badge-type ${getBadgeClass(q.questionType)}">${q.questionType}</span></td>
            <td>${correctDisplay}</td>
        </tr>`;
    }
})();
