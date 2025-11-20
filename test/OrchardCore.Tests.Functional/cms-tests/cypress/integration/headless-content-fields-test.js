/// <reference types="Cypress" />
import { generateTenantInfo } from 'cypress-orchardcore/dist/utils';

describe('Headless Recipe Content Type and Fields test', function () {
    let tenant;
    const contentTypeName = 'TestContentType';

    before(() => {
        tenant = generateTenantInfo("Headless")
        cy.newTenant(tenant);
    })

    it('Admin login should work', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin`);
        cy.get('.menu-admin').should('have.id', 'adminMenu')
    })

    it('Should create a new content type', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/ContentTypes`);
        
        // Click the create content type button
        cy.btnCreateClick();
        
        // Fill in content type name and display name
        cy.get('#DisplayName').type(contentTypeName);
        
        // Save the content type
        cy.btnCreateClick();
        
        // Verify we're on the edit page
        cy.url().should('include', '/Admin/ContentTypes/Edit/' + contentTypeName);
    })

    it('Should add a TextField to the content type', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/ContentTypes/Edit/${contentTypeName}`);
        
        // Click Add Field button
        cy.contains('a', 'Add Field').click();
        
        // Select TextField using radio button
        cy.get('input[type="radio"][value="TextField"]').check();
        
        // Enter field name
        cy.get('#DisplayName').type('Text Field');
        
        // Submit the field
        cy.btnSaveClick();
        
        // Wait for the page to reload
        cy.url().should('include', '/Admin/ContentTypes/Edit/' + contentTypeName);
        
        // Verify the field was added
        cy.get('.list-group').should('contain', 'Text Field');
    })

    it('Should add a BooleanField to the content type', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/ContentTypes/Edit/${contentTypeName}`);
        
        cy.contains('a', 'Add Field').click();
        cy.get('input[type="radio"][value="BooleanField"]').check();
        cy.get('#DisplayName').type('Boolean Field');
        cy.btnSaveClick();
        
        cy.url().should('include', '/Admin/ContentTypes/Edit/' + contentTypeName);
        cy.get('.list-group').should('contain', 'Boolean Field');
    })

    it('Should add a NumericField to the content type', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/ContentTypes/Edit/${contentTypeName}`);
        
        cy.contains('a', 'Add Field').click();
        cy.get('input[type="radio"][value="NumericField"]').check();
        cy.get('#DisplayName').type('Numeric Field');
        cy.btnSaveClick();
        
        cy.url().should('include', '/Admin/ContentTypes/Edit/' + contentTypeName);
        cy.get('.list-group').should('contain', 'Numeric Field');
    })

    it('Should add a DateField to the content type', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/ContentTypes/Edit/${contentTypeName}`);
        
        cy.contains('a', 'Add Field').click();
        cy.get('input[type="radio"][value="DateField"]').check();
        cy.get('#DisplayName').type('Date Field');
        cy.btnSaveClick();
        
        cy.url().should('include', '/Admin/ContentTypes/Edit/' + contentTypeName);
        cy.get('.list-group').should('contain', 'Date Field');
    })

    it('Should add a DateTimeField to the content type', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/ContentTypes/Edit/${contentTypeName}`);
        
        cy.contains('a', 'Add Field').click();
        cy.get('input[type="radio"][value="DateTimeField"]').check();
        cy.get('#DisplayName').type('DateTime Field');
        cy.btnSaveClick();
        
        cy.url().should('include', '/Admin/ContentTypes/Edit/' + contentTypeName);
        cy.get('.list-group').should('contain', 'DateTime Field');
    })

    it('Should add a TimeField to the content type', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/ContentTypes/Edit/${contentTypeName}`);
        
        cy.contains('a', 'Add Field').click();
        cy.get('input[type="radio"][value="TimeField"]').check();
        cy.get('#DisplayName').type('Time Field');
        cy.btnSaveClick();
        
        cy.url().should('include', '/Admin/ContentTypes/Edit/' + contentTypeName);
        cy.get('.list-group').should('contain', 'Time Field');
    })

    it('Should add an HtmlField to the content type', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/ContentTypes/Edit/${contentTypeName}`);
        
        cy.contains('a', 'Add Field').click();
        cy.get('input[type="radio"][value="HtmlField"]').check();
        cy.get('#DisplayName').type('Html Field');
        cy.btnSaveClick();
        
        cy.url().should('include', '/Admin/ContentTypes/Edit/' + contentTypeName);
        cy.get('.list-group').should('contain', 'Html Field');
    })

    it('Should add a MarkdownField to the content type', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/ContentTypes/Edit/${contentTypeName}`);
        
        cy.contains('a', 'Add Field').click();
        cy.get('input[type="radio"][value="MarkdownField"]').check();
        cy.get('#DisplayName').type('Markdown Field');
        cy.btnSaveClick();
        
        cy.url().should('include', '/Admin/ContentTypes/Edit/' + contentTypeName);
        cy.get('.list-group').should('contain', 'Markdown Field');
    })

    it('Should add a LinkField to the content type', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/ContentTypes/Edit/${contentTypeName}`);
        
        cy.contains('a', 'Add Field').click();
        cy.get('input[type="radio"][value="LinkField"]').check();
        cy.get('#DisplayName').type('Link Field');
        cy.btnSaveClick();
        
        cy.url().should('include', '/Admin/ContentTypes/Edit/' + contentTypeName);
        cy.get('.list-group').should('contain', 'Link Field');
    })

    it('Should add a ContentPickerField to the content type', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/ContentTypes/Edit/${contentTypeName}`);
        
        cy.contains('a', 'Add Field').click();
        cy.get('input[type="radio"][value="ContentPickerField"]').check();
        cy.get('#DisplayName').type('ContentPicker Field');
        cy.btnSaveClick();
        
        cy.url().should('include', '/Admin/ContentTypes/Edit/' + contentTypeName);
        cy.get('.list-group').should('contain', 'ContentPicker Field');
    })

    it('Should add a UserPickerField to the content type', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/ContentTypes/Edit/${contentTypeName}`);
        
        cy.contains('a', 'Add Field').click();
        cy.get('input[type="radio"][value="UserPickerField"]').check();
        cy.get('#DisplayName').type('UserPicker Field');
        cy.btnSaveClick();
        
        cy.url().should('include', '/Admin/ContentTypes/Edit/' + contentTypeName);
        cy.get('.list-group').should('contain', 'UserPicker Field');
    })

    it('Should add a MultiTextField to the content type', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/ContentTypes/Edit/${contentTypeName}`);
        
        cy.contains('a', 'Add Field').click();
        cy.get('input[type="radio"][value="MultiTextField"]').check();
        cy.get('#DisplayName').type('MultiText Field');
        cy.btnSaveClick();
        
        cy.url().should('include', '/Admin/ContentTypes/Edit/' + contentTypeName);
        cy.get('.list-group').should('contain', 'MultiText Field');
    })

    it('Should add a YoutubeField to the content type', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/ContentTypes/Edit/${contentTypeName}`);
        
        cy.contains('a', 'Add Field').click();
        cy.get('input[type="radio"][value="YoutubeField"]').check();
        cy.get('#DisplayName').type('Youtube Field');
        cy.btnSaveClick();
        
        cy.url().should('include', '/Admin/ContentTypes/Edit/' + contentTypeName);
        cy.get('.list-group').should('contain', 'Youtube Field');
    })

    it('Should make the content type creatable', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/ContentTypes/Edit/${contentTypeName}`);
        
        // Check the Creatable checkbox if not already checked
        cy.get('input[name="Creatable"]').then($checkbox => {
            if (!$checkbox.is(':checked')) {
                cy.get('input[name="Creatable"]').check();
            }
        });
        
        // Save the content type
        cy.btnSaveClick();
        
        // Wait for success message
        cy.get('.message-success').should('be.visible');
    })

    it('Should create a content item with all fields', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/Contents/ContentItems`);
        
        // Click the New button and select our content type
        cy.get('#new-dropdown').click();
        cy.get(`a[href*="Create/${contentTypeName}"]`).click();
        
        // Fill in the text field
        cy.get('input[name*="TextField"]').first().type('Test text value');
        
        // Check the boolean field
        cy.get('input[name*="BooleanField"]').first().check();
        
        // Fill in the numeric field
        cy.get('input[name*="NumericField"]').first().type('42');
        
        // Fill in the date field
        cy.get('input[name*="DateField"]').first().type('2024-01-15');
        
        // Fill in the datetime field
        cy.get('input[name*="DateTimeField"]').first().type('2024-01-15T10:30');
        
        // Fill in the time field
        cy.get('input[name*="TimeField"]').first().type('14:30');
        
        // Fill in the markdown field
        cy.get('textarea[name*="MarkdownField"]').first().type('# Markdown content');
        
        // Fill in the link field URL
        cy.get('input[name*="LinkField.Url"]').first().type('https://example.com');
        
        // Fill in the link field text
        cy.get('input[name*="LinkField.Text"]').first().type('Example Link');
        
        // Fill in the youtube field
        cy.get('input[name*="YoutubeField"]').first().type('https://www.youtube.com/watch?v=dQw4w9WgXcQ');
        
        // Publish the content item
        cy.btnPublishClick();
        
        // Verify we're redirected to the content items list
        cy.url().should('include', '/Admin/Contents/ContentItems');
        
        // Verify the content item was created
        cy.get('.list-group').should('contain', 'TestContentType');
    })

    it('Should edit the content item', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/Contents/ContentItems`);
        
        // Find and click the edit button for our content type
        cy.contains('.list-group-item', 'TestContentType').within(() => {
            cy.get('a[href*="Edit"]').first().click();
        });
        
        // Update the text field
        cy.get('input[name*="TextField"]').first().clear().type('Updated text value');
        
        // Update the numeric field
        cy.get('input[name*="NumericField"]').first().clear().type('99');
        
        // Publish the changes
        cy.btnPublishClick();
        
        // Verify we're redirected back to the list
        cy.url().should('include', '/Admin/Contents/ContentItems');
    })

    it('Should verify the content item was updated', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/Contents/ContentItems`);
        
        // Find and click the edit button again
        cy.contains('.list-group-item', 'TestContentType').within(() => {
            cy.get('a[href*="Edit"]').first().click();
        });
        
        // Verify the updated values
        cy.get('input[name*="TextField"]').first().should('have.value', 'Updated text value');
        cy.get('input[name*="NumericField"]').first().should('have.value', '99');
    })
});
