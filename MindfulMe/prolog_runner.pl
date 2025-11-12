:- use_module(library(http/json)).
:- initialization(main, main).

main(Argv) :-
    ( Argv = [S,M,H|_] ->
        % convert atom args to numbers (command-line args are atoms, not strings)
        atom_number(S, SN),
        atom_number(M, MN),
        atom_number(H, HN),

        % load the knowledge base (FIXED: correct spelling)
        consult('recommendations.pl'),

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