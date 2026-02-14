// ===== api.js — AJAX Fetcher Functions with Basic Auth =====
const QuizApi = (() => {
    // Hardcoded Basic Auth — verifies API calls originate from this app
    const AUTH_HEADER = 'Basic ' + btoa('admin:quiz@123');

    async function request(url, options = {}) {

        const headers = {
            'Content-Type': 'application/json',
            'Authorization': AUTH_HEADER,
            ...(options.headers || {})
        };

        try {
            const res = await fetch(url, { ...options, headers });

            if (res.status === 401) {
                return { ok: false, data: null, error: 'API authentication failed.' };
            }

            if (res.status === 204) return { ok: true, data: [], error: null };
            if (!res.ok) {
                const body = await res.json().catch(() => ({}));
                return { ok: false, data: null, error: body.error || body.errors || res.statusText };
            }

            const data = await res.json();
            return { ok: true, data, error: null };
        } catch (err) {
            return { ok: false, data: null, error: err.message };
        }
    }

    return {
        fetchAllQuestions: () => request('/api/questions'),

        fetchAllQuestionsDetail: () => request('/api/questions/detail'),

        fetchQuestionByOrder: (orderNo) => request(`/api/questions/${orderNo}`),

        postResponse: (sessionId, questionId, answerText) =>
            request('/api/responses', {
                method: 'POST',
                body: JSON.stringify({ sessionId, questionId, answerText })
            }),

        fetchSessionResponses: (sessionId) => request(`/api/responses/${sessionId}`),

        fetchSummary: (sessionId) => request(`/api/summary/${sessionId}`)
    };
})();
