// ===== quiz.js — Quiz Page Logic (Optimised) =====
(function () {
    'use strict';

    let currentIndex = 0;
    let questions = [];    // all questions cached
    let responseCache = {};

    // ----- Session -----
    function getOrCreateSessionId() {
        let sid = localStorage.getItem('quiz_session_id');
        if (!sid) {
            sid = crypto.randomUUID();
            localStorage.setItem('quiz_session_id', sid);
        }
        return sid;
    }

    // ----- Init — single parallel load -----
    $(document).ready(async function () {
        const sessionId = getOrCreateSessionId();

        // Fetch questions + saved responses in parallel (2 calls instead of 3)
        const [qRes, rRes] = await Promise.all([
            QuizApi.fetchAllQuestionsDetail(),
            QuizApi.fetchSessionResponses(sessionId)
        ]);

        if (!qRes.ok || !qRes.data || qRes.data.length === 0) {
            $('#question-text').text('Failed to load questions.');
            return;
        }

        questions = qRes.data;

        if (rRes.ok && rRes.data) {
            rRes.data.forEach(r => responseCache[r.questionId] = r.answerText);
        }

        renderCurrent();

        $('#btn-next').on('click', onNextClick);
        $('#btn-prev').on('click', onPreviousClick);
        $('#btn-submit').on('click', onSubmitClick);
    });

    // ----- Render current question from cache (instant) -----
    function renderCurrent() {
        const q = questions[currentIndex];
        if (!q) return;

        $('#question-text').text(`Q${q.orderNo}. ${q.questionText}`);
        $('#progress-badge').text(`Q ${q.orderNo} / ${questions.length}`);

        const $input = $('#quiz-input').empty();
        const saved = responseCache[q.questionId] || null;

        switch (q.questionType) {
            case 'SingleChoice': $input.append(renderRadioGroup(q.options, saved)); break;
            case 'MultipleChoice': $input.append(renderCheckboxGroup(q.options, saved)); break;
            case 'ShortAnswer': $input.append(renderTextbox(saved)); break;
            case 'PhoneNumber': $input.append(renderPhoneInput(saved)); break;
            case 'LongAnswer': $input.append(renderTextarea(saved)); break;
        }

        clearValidationError();
        updateNavButtons();
        updateProgress();
    }

    // ----- Renderers -----
    function renderRadioGroup(options, saved) {
        const $div = $('<div>');
        options.forEach(opt => {
            const checked = saved === String(opt.optionId) ? 'checked' : '';
            $div.append(`
                <div class="form-check-custom">
                    <input class="quiz-radio" type="radio" name="answer"
                           value="${opt.optionId}" id="opt-${opt.optionId}" ${checked}>
                    <label for="opt-${opt.optionId}">${opt.optionText}</label>
                </div>
            `);
        });
        return $div;
    }

    function renderCheckboxGroup(options, saved) {
        const selectedIds = saved ? saved.split(',') : [];
        const $div = $('<div>');
        options.forEach(opt => {
            const checked = selectedIds.includes(String(opt.optionId)) ? 'checked' : '';
            $div.append(`
                <div class="form-check-custom">
                    <input class="quiz-checkbox" type="checkbox"
                           value="${opt.optionId}" id="opt-${opt.optionId}" ${checked}>
                    <label for="opt-${opt.optionId}">${opt.optionText}</label>
                </div>
            `);
        });
        return $div;
    }

    function renderTextbox(saved) {
        return $(`<input type="text" class="form-control-custom" id="answer-text"
                         placeholder="Type your answer..." value="${saved || ''}">`);
    }

    function renderPhoneInput(saved) {
        return $(`<input type="text" class="form-control-custom" id="answer-phone"
                         inputmode="numeric" maxlength="10"
                         placeholder="10-digit phone number" value="${saved || ''}">`);
    }

    function renderTextarea(saved) {
        return $(`<textarea class="form-control-custom" id="answer-long" rows="4"
                            placeholder="Write at least 10 characters...">${saved || ''}</textarea>`);
    }

    // ----- Get Current Answer -----
    function getCurrentAnswer() {
        const q = questions[currentIndex];
        if (!q) return '';
        switch (q.questionType) {
            case 'SingleChoice':
                return $('input.quiz-radio:checked').val() || '';
            case 'MultipleChoice':
                return $('input.quiz-checkbox:checked').map(function () { return $(this).val(); }).get().join(',');
            case 'ShortAnswer':
                return $('#answer-text').val() || '';
            case 'PhoneNumber':
                return $('#answer-phone').val() || '';
            case 'LongAnswer':
                return $('#answer-long').val() || '';
            default:
                return '';
        }
    }

    // ----- Validation -----
    function validateCurrentInput() {
        const q = questions[currentIndex];
        if (!q) return false;
        clearValidationError();
        const answer = getCurrentAnswer();

        switch (q.questionType) {
            case 'SingleChoice':
                if (!answer) { showValidationError('Please select an option.'); return false; }
                break;
            case 'MultipleChoice':
                if (!answer) { showValidationError('Please select at least one option.'); return false; }
                break;
            case 'ShortAnswer':
                if (!answer.trim()) { showValidationError('Answer cannot be blank.'); return false; }
                break;
            case 'PhoneNumber':
                if (!/^\d{10}$/.test(answer.trim())) { showValidationError('Phone number must be exactly 10 digits.'); return false; }
                break;
            case 'LongAnswer':
                if (answer.trim().length < 10) { showValidationError('Answer must be at least 10 characters.'); return false; }
                break;
        }
        return true;
    }

    function showValidationError(msg) {
        $('#validation-error').text(msg).show();
    }

    function clearValidationError() {
        $('#validation-error').text('').hide();
    }

    // ----- Progress -----
    function updateProgress() {
        const pct = Math.round(((currentIndex + 1) / questions.length) * 100);
        $('#progress-fill').css('width', pct + '%');
    }

    // ----- Navigation -----
    async function onNextClick() {
        if (!validateCurrentInput()) return;
        if (!await saveCurrentResponse()) return; // Stop if save fails
        currentIndex++;
        renderCurrent();
    }

    function onPreviousClick() {
        if (currentIndex > 0) {
            currentIndex--;
            renderCurrent();
        }
    }

    async function onSubmitClick() {
        if (!validateCurrentInput()) return;
        if (!await saveCurrentResponse()) return; // Stop if save fails
        const sessionId = getOrCreateSessionId();
        window.location.href = `/Home/Summary?sessionId=${sessionId}`;
    }

    async function saveCurrentResponse() {
        const q = questions[currentIndex];
        const answer = getCurrentAnswer();
        const sessionId = getOrCreateSessionId();

        const res = await QuizApi.postResponse(sessionId, q.questionId, answer);
        if (!res.ok) {
            const errs = res.data?.errors || res.error;
            showValidationError(Array.isArray(errs) ? errs.join(' ') : errs);
            return false;
        }

        responseCache[q.questionId] = answer;
        return true;
    }

    function updateNavButtons() {
        $('#btn-prev').prop('disabled', currentIndex <= 0);

        if (currentIndex >= questions.length - 1) {
            $('#btn-next').hide();
            $('#btn-submit').show();
        } else {
            $('#btn-next').show();
            $('#btn-submit').hide();
        }
    }
})();
