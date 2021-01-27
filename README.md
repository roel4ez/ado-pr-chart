# ado-pr-chart

Quickly generate a PR chart for your ADO repositories, grouped per month.

![chart](README.png)

## Usage

Use the command line to pass the following arguments:

- `--ado-org`: the name of the Organization in ADO
- `--ado-proj` the name of the Project in ADO
- `--ado-repo` the UUID of the Repository in ADO
- `--ado-pat`: a PAT for ADO with scope vso.code
- (optional) `--pr-status`: the status of the requested PR [completed, active, abandoned, any]
- (optional) `--page-size`: the number of items you wish to receive (REST API defaults to 100)

or create an `appsettings.json` file based on the template and use the `-f` flag to run.

### How to find the project UUID

//TODO

## Open Topics

- [ ] save the image to a file (currently only displays the link)
- [ ] make colors customizable
- [ ] provide ability to choose fonts, colors and size
- [ ] decent error handling
