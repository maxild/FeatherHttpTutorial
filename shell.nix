let
  # using niv
  sources = import ./nix/sources.nix;
  pkgs = import sources.nixpkgs {};
in
  pkgs.mkShell {
    name = "devshell";
    buildInputs = [
      # versions are node/npm
      # LTS (16.4.0/6.14.11)
      pkgs.nodejs-14_x
      # Current (15.11.0/7.5.3)
      # pkgs.nodejs-15_x
    ];
    # shellHook = ''
    #   export PATH="$PWD/node_modules/.bin/:$PATH"
    # '';
  }