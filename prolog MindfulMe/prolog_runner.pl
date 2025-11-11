:- use_module(library(http/json)).
:- initialization(main, main).

main(Argv) :-
    ( Argv = [S,M,H|_] ->
        % convert string args to numbers
        number_string(SN, S),
        number_string(MN, M),
        number_string(HN, H),

        % Get the directory of this script and load KB from there
        prolog_load_context(directory, Dir),
        atomic_list_concat([Dir, '/reccomendations.pl'], KBPath),
        (   exists_file(KBPath)
        ->  consult(KBPath)
        ;   % Fallback: try current directory
            consult('reccomendations.pl')
        ),

        % set dynamic facts for this run
        (retractall(stress_score(_)) ; true),
        (retractall(mood_score(_)) ; true),
        (retractall(sleep_hours(_)) ; true),
        assertz(stress_score(SN)),
        assertz(mood_score(MN)),
        assertz(sleep_hours(HN)),

        % query recommendations and output JSON
        findall(R, recommend(R), Rs),
        json_write_dict(current_output, _{recommendations: Rs})
    ; 
        json_write_dict(current_output, _{error: "missing-arguments"})
    ),
    halt(0).