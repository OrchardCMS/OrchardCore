$(document).ready(function () {	
	$(".ta-left-sidebar-nano").nanoScroller();

    $("ul.menu-admin > li").click(function () {		
        $(this).siblings().find("> ul").removeClass('open');
        $(this).find("> ul").addClass("open");
		// Not sure why the timeout is needed.
		// Maybe because height of .nano-content div is not set yet.
		setTimeout(function(){
			$(".ta-left-sidebar-nano").nanoScroller();
		}, 200);
    })
});