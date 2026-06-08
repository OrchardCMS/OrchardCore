from playwright.sync_api import sync_playwright

with sync_playwright() as p:
    browser = p.chromium.launch(headless=True)
    page = browser.new_page(viewport={"width": 1280, "height": 900})

    # Check current state
    page.goto('http://localhost:5510')
    page.wait_for_load_state('networkidle')
    page.wait_for_timeout(2000)
    print(f"Home URL: {page.url}")
    print(f"Home title: {page.title()}")
    page.screenshot(path='/tmp/check_home.png', full_page=True)

    # Try login
    page.goto('http://localhost:5510/Login')
    page.wait_for_load_state('networkidle')
    page.wait_for_timeout(2000)
    print(f"Login URL: {page.url}")
    print(f"Login title: {page.title()}")

    # Check form fields
    inputs = page.locator('input')
    print(f"Input count: {inputs.count()}")
    for i in range(min(inputs.count(), 10)):
        inp = inputs.nth(i)
        name = inp.get_attribute('name') or ''
        itype = inp.get_attribute('type') or ''
        iid = inp.get_attribute('id') or ''
        print(f"  Input {i}: name={name}, type={itype}, id={iid}")

    page.screenshot(path='/tmp/check_login.png', full_page=True)

    # Fill and submit
    uname = page.locator('input[type="text"], input[name*="User"], input[id*="User"]').first
    pwd = page.locator('input[type="password"]').first

    if uname.count() > 0 and pwd.count() > 0:
        uname.fill('admin')
        pwd.fill('Password1!')
        page.screenshot(path='/tmp/check_login_filled.png', full_page=True)
        page.locator('button[type="submit"], input[type="submit"]').first.click()
        page.wait_for_load_state('networkidle')
        page.wait_for_timeout(3000)
        print(f"After login URL: {page.url}")
        page.screenshot(path='/tmp/check_after_login.png', full_page=True)

    # Now check admin
    page.goto('http://localhost:5510/Admin')
    page.wait_for_load_state('networkidle')
    page.wait_for_timeout(2000)
    print(f"Admin URL: {page.url}")
    page.screenshot(path='/tmp/check_admin.png', full_page=True)

    # Features page
    page.goto('http://localhost:5510/Admin/Features')
    page.wait_for_load_state('networkidle')
    page.wait_for_timeout(2000)
    page_content = page.content()
    print(f"Features page has 'Exam': {'Exam' in page_content}")
    page.screenshot(path='/tmp/check_features.png', full_page=True)

    # Search for Exam
    search = page.locator('#search-box, input[type="search"], input[placeholder*="Search"]')
    print(f"Search input count: {search.count()}")
    if search.count() > 0:
        search.first.fill('Exam')
        page.wait_for_timeout(2000)
        page.screenshot(path='/tmp/check_features_search.png', full_page=True)

    browser.close()
