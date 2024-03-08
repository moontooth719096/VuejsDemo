export const myMixin = {
    methods: {
        isWhiteSpace: function (text) {
            const regex = /^\s*$/;
            return regex.test(text);
        }
    }
};