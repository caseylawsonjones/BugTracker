	// widget toggle expand
	var affectedElement = $('.widget-content');

	$('.widget .btn-toggle-expand').clickToggle(
		function(e) {
			e.preventDefault();

			// if has scroll
			if( $(this).parents('.widget').find('.slimScrollDiv').length > 0 ) {
				affectedElement = $('.slimScrollDiv');
			}

			$(this).parents('.widget').find(affectedElement).slideUp(300);
			$(this).find('i.fa-chevron-up').toggleClass('fa-chevron-down');
		},
		function(e) {
			e.preventDefault();

			// if has scroll
			if( $(this).parents('.widget').find('.slimScrollDiv').length > 0 ) {
				affectedElement = $('.slimScrollDiv');
			}

			$(this).parents('.widget').find(affectedElement).slideDown(300);
			$(this).find('i.fa-chevron-up').toggleClass('fa-chevron-down');
		}
	);