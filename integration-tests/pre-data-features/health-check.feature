Feature: The library offers the ability to check the status of the Elasticsearch index.

## NOTE: Tests in this suite are meant to be run *BEFORE* data is loaded.

  Background:
    * url apiHost


  Scenario: The index does not exist.

    # Missing index will be slow. Give it three minutes.
    * configure readTimeout = 180000

    Given path 'HealthCheck'
    When method GET
    Then status 200
    And match response == 'false'
