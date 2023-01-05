# Tibber Robot Service

Calculates number of unique visited positions for a simulated robot moving on a grid.

## Usage
Start using docker-compose:
```
docker-compose up
```
A request using the example input:
```
curl --location --request POST 'https://localhost:5000/tibber-developer-test/enter-path' --header 'Content-Type: application/json' --data-raw '{"start": {"x": 10,"y": 22},"commmands": [{"direction":"east","steps": 2},{"direction": "north","steps": 1}]}'
```
returns
```
{
    "id": "8dddfdaa-846e-4f1a-b396-66f0a938f864",
    "timestamp": "2023-01-05T23:01:17.2623736Z",
    "commands": 0,
    "result": 1,
    "duration": 0.0002332
}
```
and stores the result to a postgres instance.

## Assumptions
* The starting position is cleaned before the first movement is done

## Considerations
* Used structure fit for larger project for extensibility
* Wrote several unit tests for unique position counting to validate solution
* Focused on writing clean performant code