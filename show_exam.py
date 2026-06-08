from playwright.sync_api import sync_playwright

with sync_playwright() as p:
    browser = p.chromium.launch(headless=True)
    page = browser.new_page(viewport={"width": 1280, "height": 900})

    # Login first
    page.goto('http://localhost:5510/Login')
    page.wait_for_load_state('networkidle')
    uname = page.locator('input[name="UserName"], input[id="UserName"]')
    pwd = page.locator('input[name="Password"], input[id="Password"]')
    if uname.count() > 0:
        uname.first.fill('admin')
        pwd.first.fill('Password1!')
        page.locator('button[type="submit"], input[type="submit"]').first.click()
        page.wait_for_load_state('networkidle')
        page.wait_for_timeout(2000)

    # 1. Admin Dashboard
    page.goto('http://localhost:5510/Admin')
    page.wait_for_load_state('networkidle')
    page.wait_for_timeout(2000)
    page.screenshot(path='/tmp/01_admin_dashboard.png', full_page=True)
    print("1. Admin Dashboard captured")

    # 2. Enable Exam feature
    page.goto('http://localhost:5510/Admin/Features')
    page.wait_for_load_state('networkidle')
    page.wait_for_timeout(2000)
    search = page.locator('#search-box')
    if search.count() > 0:
        search.fill('Exam')
        page.wait_for_timeout(1500)
    page.screenshot(path='/tmp/02_features_exam.png', full_page=True)
    print("2. Features page captured")

    # Enable Exam
    enable_form = page.locator('form[action*="OrchardCore.Exam"]')
    if enable_form.count() > 0:
        enable_form.first.locator('button, input[type="submit"]').click()
        page.wait_for_load_state('networkidle')
        page.wait_for_timeout(3000)
        print("Exam feature enabled!")
    else:
        print("Exam feature form not found, may already be enabled")

    page.screenshot(path='/tmp/03_exam_enabled.png', full_page=True)

    # 3. Admin menu with Exam
    page.goto('http://localhost:5510/Admin')
    page.wait_for_load_state('networkidle')
    page.wait_for_timeout(2000)
    page.screenshot(path='/tmp/04_admin_with_exam.png', full_page=True)
    print("3. Admin with Exam menu captured")

    # 4. Questions list
    page.goto('http://localhost:5510/Admin/Contents/ContentItems?contentTypeId=Question')
    page.wait_for_load_state('networkidle')
    page.wait_for_timeout(2000)
    page.screenshot(path='/tmp/05_questions_list.png', full_page=True)
    print("4. Questions list captured")

    # 5. Create Question form
    page.goto('http://localhost:5510/Admin/Contents/Create?contentTypeId=Question')
    page.wait_for_load_state('networkidle')
    page.wait_for_timeout(2000)
    page.screenshot(path='/tmp/06_create_question.png', full_page=True)
    print("5. Create Question form captured")

    # 6. Create ExamPaper form
    page.goto('http://localhost:5510/Admin/Contents/Create?contentTypeId=ExamPaper')
    page.wait_for_load_state('networkidle')
    page.wait_for_timeout(2000)
    page.screenshot(path='/tmp/07_create_paper.png', full_page=True)
    print("6. Create ExamPaper form captured")

    # 7. Create ExamAssignment form
    page.goto('http://localhost:5510/Admin/Contents/Create?contentTypeId=ExamAssignment')
    page.wait_for_load_state('networkidle')
    page.wait_for_timeout(2000)
    page.screenshot(path='/tmp/08_create_assignment.png', full_page=True)
    print("7. Create ExamAssignment form captured")

    # 8. ExamRecord list
    page.goto('http://localhost:5510/Admin/Contents/ContentItems?contentTypeId=ExamRecord')
    page.wait_for_load_state('networkidle')
    page.wait_for_timeout(2000)
    page.screenshot(path='/tmp/09_records_list.png', full_page=True)
    print("8. ExamRecord list captured")

    print("\nAll screenshots saved to /tmp/")
    browser.close()
