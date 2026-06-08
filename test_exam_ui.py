from playwright.sync_api import sync_playwright

with sync_playwright() as p:
    browser = p.chromium.launch(headless=True)
    page = browser.new_page(viewport={"width": 1280, "height": 900})

    # Step 1: Setup
    page.goto('http://localhost:5510')
    page.wait_for_load_state('networkidle')
    page.screenshot(path='/tmp/step0_home.png', full_page=True)

    if page.locator('input[name="SiteName"]').count() > 0:
        print("Setup wizard detected, filling...")
        page.fill('input[name="SiteName"]', 'Exam Test Site')
        recipe_select = page.locator('select[name="RecipeName"]')
        if recipe_select.count() > 0:
            recipe_select.select_option('Blog')
        page.fill('input[name="UserName"]', 'admin')
        page.fill('input[name="Email"]', 'admin@test.com')
        page.fill('input[name="Password"]', 'Password1!')
        page.fill('input[name="PasswordConfirmation"]', 'Password1!')
        page.locator('button[type="submit"], input[type="submit"]').first.click()
        page.wait_for_load_state('networkidle')
        page.wait_for_timeout(5000)
        page.screenshot(path='/tmp/step1_setup_done.png', full_page=True)
        print("Setup completed!")
    else:
        print("Site already set up")

    # Step 2: Go to admin (may auto-redirect to login)
    page.goto('http://localhost:5510/Admin')
    page.wait_for_load_state('networkidle')
    page.wait_for_timeout(2000)

    # Check if we need to login
    current_url = page.url
    print(f"Current URL: {current_url}")

    if 'Login' in current_url or 'login' in current_url.lower():
        print("Login page detected, filling...")
        uname = page.locator('input[name="UserName"], input[id="UserName"]')
        pwd = page.locator('input[name="Password"], input[id="Password"]')
        if uname.count() > 0 and pwd.count() > 0:
            uname.first.fill('admin')
            pwd.first.fill('Password1!')
            page.locator('button[type="submit"], input[type="submit"]').first.click()
            page.wait_for_load_state('networkidle')
            page.wait_for_timeout(2000)

    page.screenshot(path='/tmp/step2_admin.png', full_page=True)
    print("Admin page loaded!")

    # Step 3: Enable Exam feature
    page.goto('http://localhost:5510/Admin/Features')
    page.wait_for_load_state('networkidle')
    page.wait_for_timeout(2000)

    # Search for Exam
    search = page.locator('#search-box')
    if search.count() > 0:
        search.fill('Exam')
        page.wait_for_timeout(1500)

    page.screenshot(path='/tmp/step3_features_search.png', full_page=True)

    # Enable Exam feature
    page_content = page.content()
    if 'Exam' in page_content:
        # Find the enable form for Exam
        forms = page.locator('form')
        for i in range(forms.count()):
            form_action = forms.nth(i).get_attribute('action') or ''
            if 'Exam' in form_action and 'Enable' in form_action:
                forms.nth(i).locator('button, input[type="submit"]').click()
                page.wait_for_load_state('networkidle')
                page.wait_for_timeout(3000)
                print("Exam feature enabled!")
                break
        else:
            print("Exam enable form not found, trying alternative...")
            # Try clicking enable button near Exam text
            enable_links = page.locator('a:has-text("Enable"), button:has-text("Enable")')
            for i in range(enable_links.count()):
                parent = enable_links.nth(i).locator('..').locator('..').locator('..')
                parent_text = parent.inner_text() if parent.count() > 0 else ''
                if 'Exam' in parent_text:
                    enable_links.nth(i).click()
                    page.wait_for_load_state('networkidle')
                    page.wait_for_timeout(3000)
                    print("Exam feature enabled via link!")
                    break
    else:
        print("Exam feature not found on features page")

    page.screenshot(path='/tmp/step4_after_enable.png', full_page=True)

    # Step 4: Admin dashboard with Exam menu
    page.goto('http://localhost:5510/Admin')
    page.wait_for_load_state('networkidle')
    page.wait_for_timeout(2000)
    page.screenshot(path='/tmp/step5_admin_menu.png', full_page=True)

    # Step 5: Questions list
    page.goto('http://localhost:5510/Admin/Contents/ContentItems?contentTypeId=Question')
    page.wait_for_load_state('networkidle')
    page.wait_for_timeout(2000)
    page.screenshot(path='/tmp/step6_questions.png', full_page=True)

    # Step 6: Create question page
    page.goto('http://localhost:5510/Admin/Contents/Create?contentTypeId=Question')
    page.wait_for_load_state('networkidle')
    page.wait_for_timeout(2000)
    page.screenshot(path='/tmp/step7_create_question.png', full_page=True)

    # Step 7: Create paper page
    page.goto('http://localhost:5510/Admin/Contents/Create?contentTypeId=ExamPaper')
    page.wait_for_load_state('networkidle')
    page.wait_for_timeout(2000)
    page.screenshot(path='/tmp/step8_create_paper.png', full_page=True)

    # Step 8: Create assignment page
    page.goto('http://localhost:5510/Admin/Contents/Create?contentTypeId=ExamAssignment')
    page.wait_for_load_state('networkidle')
    page.wait_for_timeout(2000)
    page.screenshot(path='/tmp/step9_create_assignment.png', full_page=True)

    print("\nAll screenshots saved to /tmp/")
    print("step0_home.png - Initial page / Setup wizard")
    print("step1_setup_done.png - After setup")
    print("step2_admin.png - Admin dashboard")
    print("step3_features_search.png - Features page with Exam search")
    print("step4_after_enable.png - After enabling Exam feature")
    print("step5_admin_menu.png - Admin menu with Exam")
    print("step6_questions.png - Questions list")
    print("step7_create_question.png - Create question form")
    print("step8_create_paper.png - Create exam paper form")
    print("step9_create_assignment.png - Create exam assignment form")

    browser.close()
