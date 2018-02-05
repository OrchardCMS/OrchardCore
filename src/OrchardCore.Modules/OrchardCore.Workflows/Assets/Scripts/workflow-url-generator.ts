///<reference path="../../../OrchardCore.Resources/Assets/jQuery/Typings/jquery-latest.d.ts" />

$(() => {
    const $modal = $('#workflow-url-generator-modal');
    const $copyUrlButton = $('#copy-workflow-url-button');
    const $copyUrlButtonTitle = $copyUrlButton.attr('title');

    const generateWorkflowUrl = function () {
        const generateUrl: string = $modal.data('generate-url');
        const antiforgeryHeaderName: string = $modal.data('antiforgery-header-name');
        const antiforgeryToken: string = $modal.data('antiforgery-token');
        const headers: any = {};

        headers[antiforgeryHeaderName] = antiforgeryToken;

        $.post({
            url: generateUrl,
            headers: headers
        }).done(url => {
            $('#workflow-url-text').val(url);
        });
    };

    $('#generate-workflow-url-button').on('click', e => {
        generateWorkflowUrl();
    });

    $copyUrlButton.on('mouseenter', e => {
        $copyUrlButton.attr('data-original-title', $copyUrlButtonTitle).tooltip('show');
    });

    $copyUrlButton.on('mouseleave', e => {
        $copyUrlButton.tooltip('hide');
    });

    $copyUrlButton.on('click', e => {
        $('#workflow-url-text').select();
        document.execCommand('Copy');

        const copiedTitle = $copyUrlButton.data('copied-title');
        $copyUrlButton.attr('data-original-title', copiedTitle).tooltip('show');
    });

    $modal.on('show.bs.modal', function (event) {
        generateWorkflowUrl();
    })
});