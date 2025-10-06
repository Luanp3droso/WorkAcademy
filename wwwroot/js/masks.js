// Máscara para CPF
$(document).ready(function () {
    $('.cpf-mask').inputmask('999.999.999-99');

    // Máscara para telefone
    var SPMaskBehavior = function (val) {
        return val.replace(/\D/g, '').length === 11 ? '(00) 00000-0000' : '(00) 0000-00009';
    },
        spOptions = {
            onKeyPress: function (val, e, field, options) {
                field.mask(SPMaskBehavior.apply({}, arguments), options);
            }
        };

    $('.phone-mask').inputmask(SPMaskBehavior, spOptions);

    // Máscara para CNPJ
    $('.cnpj-mask').inputmask('99.999.999/9999-99');

    // Máscara para moeda
    $('.currency-mask').inputmask('currency', {
        prefix: 'R$ ',
        groupSeparator: '.',
        alias: 'numeric',
        placeholder: '0',
        autoGroup: true,
        digits: 2,
        digitsOptional: false,
        clearMaskOnLostFocus: false
    });
});
