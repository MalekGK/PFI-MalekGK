const PHONE_INPUT_SELECTOR = "input.phone, input.phoneExt, input[type='tel'], input[name*='Phone'], input[name*='phone'], input[id*='Phone'], input[id*='phone'], input[name*='Telephone'], input[name*='telephone'], input[id*='Telephone'], input[id*='telephone'], input[name*='Tel'], input[name*='tel'], input[id*='Tel'], input[id*='tel']";
const PHONE_MASK_PATTERN = "(000)-000-0000";

function FormatPhoneMask(rawValue) {
    const digits = (rawValue || "").replace(/\D/g, "").substring(0, 10);
    if (digits.length === 0)
        return "";
    if (digits.length <= 3)
        return "(" + digits;
    if (digits.length <= 6)
        return "(" + digits.substring(0, 3) + ")-" + digits.substring(3);
    return "(" + digits.substring(0, 3) + ")-" + digits.substring(3, 6) + "-" + digits.substring(6, 10);
}

function InstallPhoneMask() {
    $(document).off("input.phoneMask blur.phoneMask", PHONE_INPUT_SELECTOR);

    $(PHONE_INPUT_SELECTOR).each(function () {
        $(this).attr("maxlength", "14");
        if (!$(this).attr("placeholder"))
            $(this).attr("placeholder", PHONE_MASK_PATTERN);
        $(this).val(FormatPhoneMask($(this).val()));
    });

    $(document).on("input.phoneMask blur.phoneMask", PHONE_INPUT_SELECTOR, function () {
        $(this).val(FormatPhoneMask($(this).val()));
    });
}

$(document).ready(function () {
    InstallPhoneMask();

    if (typeof $.fn.mask === "function")
        $('.zipcode').mask('a9a 9a9');

    $(".datepicker").datepicker({
        dateFormat: 'yy-mm-dd',
        changeMonth: true,
        changeYear: true,
        //yearRange: "-100:-15",
        dayNamesMin: ["Dim", "Lun", "Mar", "Mer", "Jeu", "Ven", "Sam"],
        monthNamesShort: ["Janv.", "Févr.", "Mars", "Avril", "Mai", "Juin", "Juil.", "Août", "Sept.", "Oct.", "Nov.", "Déc."]
    });

    /*Filter unicode hack */
    $(":input").change(function () {
        try {
            let r = $(this).val().replace(/[^\x00-\xFF]/g, "");
            $(this).val(r);
        } catch (e) { }
    });
    $("textarea").change(function () {
        try {
            let r = $(this).val().replace(/[^\x00-\xFF]/g, "");
            $(this).val(r);
        } catch (e) { }
    });

    $(".countrySelect").change((e) => {
        $(e.target).next().attr("src", "/Images_Data/Loading_icon.gif")
        $.ajax({
            url: "/CountryFlag/get?countryCode=" + $(e.target).val(),
            datatype: "application/json",
            success: json => { $(e.target).next().attr("src", json); }
        });

    })
    SummaryHandling();
})


function SummaryHandling() {

    $('summary').attr('title', 'Utilisez ctrl-clic pour développer/réduire');
    $('summary').off();
    // Toggle collapse uncollapse details
    $('summary').on('click', function (e) {
        if (e.ctrlKey) {
            if ($(this).parent().attr('open') != undefined) {
                $('details').removeAttr('open');
                e.preventDefault();
            }
            else {
                $('details').prop('open', true);
                e.preventDefault();
            }
        }
    })
}
function RestoreDetailsState() {
    //////////////////////////////////////////////////////////
    /// Install event handler
    //////////////////////////////////////////////////////////
    $("details").off();
    $("details").on('toggle', function () {
        let details_dom = $(this)[0];
        if (details_dom != undefined) {
            // Save details state
            localStorage.setItem(details_dom.id, details_dom.open);
        }
    })
    
    // Restore state of each details tags
    for (let i = 0; i < localStorage.length; i++) {
        const key = localStorage.key(i);
        // target only keys that contain "details" string
        if (key.indexOf("details") > -1) {
            let details_dom = $("#" + key)[0];
            if (details_dom != undefined)
                // all values in localstorage are stored as string
                details_dom.open = localStorage.getItem(key) == "true";
            let i = 0;
        }
    }
}

$(document).on("click", ".submitCmd", function (e) {
    e.preventDefault();
    submitMainForm();
});

function submitMainForm() {
    const submitButton = document.getElementById("submit");
    if (submitButton) {
        submitButton.click();
        return;
    }

    const form = document.querySelector("form");
    if (!form) return;

    if (typeof form.requestSubmit === "function")
        form.requestSubmit();
    else
        form.submit();
}

function InstallAutoComplete(targetId, words) {

    function split(val) {
        return val.split(/ \s*/);
    }

    function RemoveExtra(str, extra) {
        var extraLength = extra.length;
        var lastExtraIndex = str.lastIndexOf(extra);
        if ((lastExtraIndex + extraLength) == str.length)
            str = str.substring(0, str.length - extraLength);
        return str;
    }

    function extractLast(term) {
        return split(term).pop();
    }

    $("#" + targetId)
        // don't navigate away from the field on tab when selecting an item
        .bind("keydown", function (event) {
            if (event.keyCode === $.ui.keyCode.TAB && $(this).data("ui-autocomplete").menu.active) {
                event.preventDefault();
            }
        })
        .autocomplete({
            minLength: 1,
            source: function (request, response) {
                // delegate back to autocomplete, but extract the last term
                response($.ui.autocomplete.filter(words, extractLast(request.term)));
            },
            focus: function () {
                // prevent value inserted on focus
                return false;
            },
            select: function (event, ui) {
                var terms = split(this.value);
                // remove the current input
                terms.pop();
                // add the selected item
                terms.push(ui.item.value);
                // add placeholder to get the comma-and-space at the end
                terms.push("");
                this.value = RemoveExtra(terms, ",").join(" ");
                return false;
            }
        });
}

function ajaxActionCall(actionLink) {
    // Ajax Action Call to actionLink
    $.ajax({
        url: actionLink,
        method: 'GET',
        success: (data) => {
            console.log("Result: " + data);
        }
    });
}

let minKeywordLenth = 1;
function highlight(text, elem) {
    text = text.trim();
    if (text.length >= minKeywordLenth) {
        var innerHTML = elem.innerHTML;
        let startIndex = 0;

        while (startIndex < innerHTML.length) {
            var normalizedHtml = innerHTML.toLocaleLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '');
            var index = normalizedHtml.indexOf(text, startIndex);
            let highLightedText = "";
            if (index >= startIndex) {
                highLightedText = "<span class='highlight'>" + innerHTML.substring(index, index + text.length) + "</span>";
                innerHTML = innerHTML.substring(0, index) + highLightedText + innerHTML.substring(index + text.length);
                startIndex = index + highLightedText.length + 1;
            } else
                startIndex = innerHTML.length + 1;
        }
        elem.innerHTML = innerHTML;
    }
}