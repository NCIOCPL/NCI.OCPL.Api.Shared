When working with files in this repository, please follow these guidelines:

- If you don't know how do something, say you don't know. Do not make assumptions.
- If an instruction is unclear, ask for clarification.
- Write clear and concise code comments where necessary.
- Follow the existing coding style and conventions used in the repository.
- Ensure proper error handling and input validation.

- Use the modern Elastic.Clients.Elasticsearch.
- Use System.Text.Json instead of Newtonsoft.Json.
- Remove any unneeded Using statements.
- Using statements should be split into groups in this order:
  1. "Out of the box" packages.
  2. Third-party packages (e.g. the elasticsearch packages)
  3. Packages which are part of this solution.
- Within a group, Using statements should be in alphabetical order.
- When committing changes, include a brief summary of what was changed and why.