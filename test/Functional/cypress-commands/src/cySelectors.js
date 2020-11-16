function byCy(id, exact) {
  if (exact) {
    return `[data-cy="${id}"]`;
  }
  return `[data-cy^="${id}"]`;
}

Cypress.Commands.add('getByCy', (selector, exact = false) => {
  return cy.get(byCy(selector, exact));
});

Cypress.Commands.add(
  'findByCy',
  {prevSubject: 'optional'},
  (subject, selector, exact = false) => {
    return subject
      ? cy.wrap(subject).find(byCy(selector, exact))
      : cy.find(byCy(selector, exact));
  },
);
