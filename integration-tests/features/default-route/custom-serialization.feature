Feature: The API library allows for custom deserialization of data retrieved from Elasticsearch.

  Background:
    * url apiHost

  Scenario Outline: Fields which use default serialization are unaffected.

    Given path 'test/custom-serialization/', identifier
    When method get
    Then status 200
    And match $.default == 'Default serialization'

    Examples:
      | identifier    |
      | simple-string |
      | has-array     |

  Scenario Outline: Fields which use custom serialization can have special handling.

    Given path 'test/custom-serialization/', identifier
    When method get
    Then status 200
    And match $.custom == expected

    Examples:
      | identifier    | expected            |
      | simple-string | simple-string-value |
      | has-array     | first-string-value  |
